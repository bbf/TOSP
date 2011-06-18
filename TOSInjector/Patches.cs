using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil.Cil;
using System.Xml.Serialization;
using Mono.Cecil;
using System.Reflection;
using Mono.Collections.Generic;

namespace TOSP
{
    public class Patches
    {
        public HookPatch TileChange { get; set; }
    }

    public class HookPatch
    {
        public string TypeFullName { get; set; }
        public string MethodName { get; set; }
        public int Offset { get; set; }

        public List<HookCode> Code { get; set; }
    }

    [XmlInclude(typeof(StringArg)), XmlInclude(typeof(FieldRefArg)), XmlInclude(typeof(MethodRefArg)), XmlInclude(typeof(VariableRefArg)), XmlInclude(typeof(InstructionRefArg))]
    public class HookCode
    {
        [XmlIgnoreAttribute()]
        public OpCode OpCode { get; set; }

        [XmlElementAttribute("OpCode")]
        public string OpCodeName
        {
            get
            {
                if (OpCode != null)
                {
                    return OpCode.ToString().Replace('.', '_');
                }
                else
                {
                    return null;
                }
            }
            set
            {
                if (value != null)
                {
                    MemberInfo[] arrayMemberInfo = typeof(OpCodes).FindMembers(MemberTypes.Field, BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance, new MemberFilter(CaseInsensitiveSearchDelegate), value);
                    if (arrayMemberInfo.Length == 1)
                    {
                        FieldInfo fi = (FieldInfo)arrayMemberInfo[0];
                        OpCode = (OpCode)fi.GetValue(typeof(OpCodes));
                    }
                    else
                    {
                        throw new ApplicationException("Unknown OpCode: " + value);
                    }
                }
                else
                {
                    //OpCode = null;
                }
            }

        }

        static bool CaseInsensitiveSearchDelegate(MemberInfo objMemberInfo, Object objSearch)
        {
            if (objMemberInfo.Name.ToString().ToLower() == objSearch.ToString().ToLower())
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public OpCodeArg Argument { get; set; }

    }

    public abstract class OpCodeArg
    {
        protected TypeReference FindType(ModuleDefinition moduleDefinition, string typeName)
        {
            TypeReference typeRef = moduleDefinition.GetType(typeName);
            if (typeRef == null)
            {
                Type type = Type.GetType(typeName);
                if (type != null)
                {
                    typeRef = moduleDefinition.Import(type);
                }
            }
            return typeRef;
        }

        protected Boolean CompareParameters(MethodDefinition method, List<TypeReference> parameters)
        {
            if (method.Parameters.Count != parameters.Count)
            {
                return false;
            }

            for (int i = 0; i < method.Parameters.Count; i++)
            {
                if (method.Parameters[i].ParameterType.FullName != parameters[i].FullName)
                {
                    return false;
                }
            }
            return true;
        }

        protected MethodReference GetMethod(TypeReference typeRef, string methodName, List<TypeReference> args = null, int arrayCount = 0, ModuleDefinition moduleDefinition = null)
        {
            if (arrayCount > 0)
            {
                ArrayType arrayType = new ArrayType(typeRef, arrayCount);
                TypeReference arrayTypeRef = moduleDefinition.Import(new ArrayType(typeRef, arrayCount));
                return new MethodReference("Get", typeRef, arrayTypeRef);
            }
            else
            {
                if (args != null)
                {
                    return typeRef.Resolve().Methods.Single(m => m.Name == methodName && CompareParameters(m, args));
                }
                else
                {
                    return typeRef.Resolve().Methods.Single(m => m.Name == methodName);
                }
            }
        }
    }

    public class InvalidPatchException : Exception
    {
        public InvalidPatchException(string message) : base(message) { }
    }

    public class StringArg : OpCodeArg
    {
        public string Value { get; set; }

        public StringArg() { }
        public StringArg(string value)
        {
            this.Value = value;
        }
    }



    public class FieldRefArg : OpCodeArg
    {
        public string TypeName { get; set; }
        public string FieldName { get; set; }

        public FieldReference Value(ModuleDefinition moduleDefinition)
        {
            TypeReference typeRef = moduleDefinition.Types.Single(m => m.FullName == TypeName);
            FieldDefinition field = typeRef.Resolve().Fields.Single(m => m.Name == FieldName);
            return moduleDefinition.Import(field);
        }
    }


    public class MethodRefArg : OpCodeArg
    {
        public string TypeName { get; set; }
        public int ArrayCount { get; set; }
        public string MethodName { get; set; }
        public string[] ArgumentTypes { get; set; }

        public MethodReference Value(ModuleDefinition moduleDefinition)
        {
            TypeReference typeRef = FindType(moduleDefinition, TypeName);
            if (typeRef == null)
            {
                throw new InvalidPatchException(String.Format("Could not find type: {0} for MethodRefArg {1}", TypeName, this));
            }

            MethodReference method = null;

            if (ArgumentTypes == null)
            {
                method = GetMethod(typeRef, MethodName, null, ArrayCount, moduleDefinition);
            }
            else
            {
                List<TypeReference> args = new List<TypeReference>();
                foreach (string argumentType in ArgumentTypes)
                {
                    TypeReference argTypeRef = FindType(moduleDefinition, argumentType);
                    if (argTypeRef == null)
                    {
                        throw new InvalidPatchException(String.Format("Could not find type: {0} for argument in MethodRefArg {1}", argumentType, this));
                    }
                    args.Add(argTypeRef);
                }

                method = GetMethod(typeRef, MethodName, args, ArrayCount, moduleDefinition);
            }

            if (method == null)
            {
                throw new InvalidPatchException(String.Format("Could not find method: {0} for MethodRefArg {1}", MethodName, this));
            }

            return moduleDefinition.Import(method);
        }
    }

    public class InstructionRefArg : OpCodeArg
    {
        public int Offset { get; set; }

        public Instruction Value(ModuleDefinition moduleDefinition, Instruction nextInstruction)
        {
            Instruction ptr = nextInstruction;
            int counter = Offset;
            while (counter > 0)
            {
                if (ptr.Next == null)
                {
                    throw new InvalidPatchException(String.Format("Could not find next instruction: {0}/{1} for InstructionRefArg {2}", counter, Offset, this));
                }
                ptr = ptr.Next;
                counter--;

            }
            return ptr;
        }
    }

    public class VariableRefArg : OpCodeArg
    {
        public string TypeName { get; set; }
        public string MethodName { get; set; }
        public string[] ArgumentTypes { get; set; }
        public int VariableIndex { get; set; }

        public VariableDefinition Value(ModuleDefinition moduleDefinition)
        {

            TypeDefinition type = moduleDefinition.GetType(TypeName);
            if (type == null)
            {
                throw new InvalidPatchException(String.Format("Could not find type: {0} for VariableRefArg {1}", TypeName, this));
            }

            MethodReference method = null;
            if (ArgumentTypes == null)
            {
                method = type.Methods.Single(m => m.Name == MethodName);
            }
            else
            {
                List<TypeReference> args = new List<TypeReference>();
                foreach (string argumentType in ArgumentTypes)
                {
                    TypeReference argRef = moduleDefinition.GetType(argumentType);
                    Type argType = null;
                    if (argRef == null)
                    {
                        argType = Type.GetType(argumentType);
                        argRef = moduleDefinition.Import(argType);
                    }
                    if (argRef == null)
                    {
                        throw new ApplicationException("Invalid Type for Argument: " + argumentType);
                    }

                    args.Add(argRef);
                }

                method = GetMethod(type, MethodName, args);
            }

            if (method == null)
            {
                throw new InvalidPatchException(String.Format("Could not find method: {0} for VariableRefArg {1}", MethodName, this));
            }

            MethodDefinition methodDef = method.Resolve();

            if (method == null)
            {
                throw new InvalidPatchException(String.Format("Could not resolve method: {0} for VariableRefArg {1}", MethodName, this));
            }

            if (VariableIndex > methodDef.Body.Variables.Count - 1)
            {
                throw new InvalidPatchException(String.Format("Could not find variable: {0} for VariableRefArg {1}", VariableIndex, this));
            }

            return methodDef.Body.Variables[VariableIndex];
        }


    }



}
