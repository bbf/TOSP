using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System.Xml.Serialization;
using TOSP.Util;

namespace TOSP
{
    public class Injector
    {
        private const string PATCH_FILE_TEMPLATE = "Patches_{0}.xml";

        public static void Inject(string fileName, string newFileName)
        {
            var assembly = AssemblyDefinition.ReadAssembly(fileName, new ReaderParameters { ReadSymbols = true });

            var version = assembly.Name.Version.ToString();

            ModuleDefinition moduleDefinition = assembly.MainModule;

            var patchesFile = String.Format(PATCH_FILE_TEMPLATE, version);

            Console.WriteLine("File: " + patchesFile);

            XmlSerializer patchSerializer = new XmlSerializer(typeof(Patches));

            Patches p = TemporaryHardcodedPatches2();
            
            //Serializers.SerializeXML(patchesFile, patchSerializer, p);
            //p = (Patches)Serializers.DeserializeXML(patchesFile, patchSerializer);

            // TODO Find patch offsets, and store instructions, and only then apply them.
            // Because when applying patches to the same member, offsets change
            ApplyPatch(assembly.MainModule, p.TileChange);

            assembly.Write(newFileName, new WriterParameters { WriteSymbols = true });

        }

        protected static bool ApplyPatch(ModuleDefinition moduleDefinition, HookPatch hp)
        {
            TypeDefinition typeDefinition = moduleDefinition.GetType(hp.TypeFullName);
            if (typeDefinition == null)
            {
                Console.WriteLine("Could not find type: " + hp.TypeFullName);
                return true;
            }

            var method = typeDefinition.Methods.Single(m => m.Name == hp.MethodName);
            if (method == null)
            {
                Console.WriteLine("Could not find method: " + hp.MethodName);
                return true;
            }

            var il = method.Body.GetILProcessor();

            var ptr = method.Body.Instructions[hp.Offset];

            foreach (HookCode code in hp.Code)
            {
                Instruction opCode = null;

                if (code.Argument != null)
                {
                    switch (code.Argument.GetType().ToString())
                    {
                        case "TOSP.StringArg":
                            opCode = il.Create(code.OpCode, ((StringArg)code.Argument).Value);
                            break;

                        case "TOSP.MethodRefArg":
                            opCode = il.Create(code.OpCode, ((MethodRefArg)code.Argument).Value(moduleDefinition));
                            break;

                        case "TOSP.FieldRefArg":
                            opCode = il.Create(code.OpCode, ((FieldRefArg)code.Argument).Value(moduleDefinition));
                            break;

                        case "TOSP.InstructionRefArg":
                            opCode = il.Create(code.OpCode, ((InstructionRefArg)code.Argument).Value(moduleDefinition, ptr));
                            break;

                        case "TOSP.VariableRefArg":
                            opCode = il.Create(code.OpCode, ((VariableRefArg)code.Argument).Value(moduleDefinition));
                            break;

                        default:
                            throw new ApplicationException("Could not apply HookCode with argument: " + code.Argument.GetType().ToString());
                    }
                }
                else
                {
                    opCode = il.Create(code.OpCode);
                }

                if (opCode == null)
                {
                    throw new ApplicationException("Could not apply HookCode: " + code);
                }

                il.InsertBefore(ptr, opCode);
            }


            return false;
        }

        public static Patches TemporaryHardcodedPatches2()
        {
            HookCode code;
            FieldRefArg fra;
            MethodRefArg mra;
            VariableRefArg vra;
            InstructionRefArg ira;

            Patches p = new Patches();
            p.TileChange = new HookPatch();
            p.TileChange.Offset = 3351;
            p.TileChange.Code = new List<HookCode>();
            p.TileChange.TypeFullName = "Terraria.messageBuffer";
            p.TileChange.MethodName = "GetData";

            code = new HookCode();
            code.OpCode = OpCodes.Ldstr;
            code.Argument = new StringArg("TileChange");
            p.TileChange.Code.Add(code);

            code = new HookCode();
            code.OpCode = OpCodes.Call;
            mra = new MethodRefArg();
            mra.TypeName = "System.Console";
            mra.MethodName = "WriteLine";
            mra.ArgumentTypes = new[] { "System.String" };
            code.Argument = mra;
            p.TileChange.Code.Add(code);

            code = new HookCode();
            code.OpCode = OpCodes.Ldloc_S;
            vra = new VariableRefArg();
            vra.TypeName = "Terraria.messageBuffer";
            vra.MethodName = "GetData";
            vra.ArgumentTypes = new[] { "System.Int32", "System.Int32" };
            vra.VariableIndex = 20;
            code.Argument = vra;
            p.TileChange.Code.Add(code);

            code = new HookCode();
            code.OpCode = OpCodes.Ldloc_S;
            vra = new VariableRefArg();
            vra.TypeName = "Terraria.messageBuffer";
            vra.MethodName = "GetData";
            vra.ArgumentTypes = new[] { "System.Int32", "System.Int32" };
            vra.VariableIndex = 21;
            code.Argument = vra;
            p.TileChange.Code.Add(code);

            code = new HookCode();
            code.OpCode = OpCodes.Ldarg_0;
            p.TileChange.Code.Add(code); code = new HookCode();

            code.OpCode = OpCodes.Ldfld;
            fra = new FieldRefArg();
            fra.TypeName = "Terraria.messageBuffer";
            fra.FieldName = "whoAmI";
            code.Argument = fra;
            p.TileChange.Code.Add(code);

            code = new HookCode();
            code.OpCode = OpCodes.Ldloc_S;
            vra = new VariableRefArg();
            vra.TypeName = "Terraria.messageBuffer";
            vra.MethodName = "GetData";
            vra.ArgumentTypes = new[] { "System.Int32", "System.Int32" };
            vra.VariableIndex = 44;
            code.Argument = vra;
            p.TileChange.Code.Add(code);

            code = new HookCode();
            code.OpCode = OpCodes.Ldloc_S;
            vra = new VariableRefArg();
            vra.TypeName = "Terraria.messageBuffer";
            vra.MethodName = "GetData";
            vra.ArgumentTypes = new[] { "System.Int32", "System.Int32" };
            vra.VariableIndex = 45;
            code.Argument = vra;
            p.TileChange.Code.Add(code);

            code = new HookCode();
            code.OpCode = OpCodes.Call;
            mra = new MethodRefArg();
            mra.TypeName = "TOSP.Hook";
            mra.MethodName = "OnTileChangeHook";
            mra.ArgumentTypes = new[] { "System.Int32", "System.Int32", "System.Int32", "System.Byte", "System.Boolean" };
            code.Argument = mra;
            p.TileChange.Code.Add(code);

            code = new HookCode();
            code.OpCode = OpCodes.Brfalse_S;
            ira = new InstructionRefArg();
            ira.Offset = 0;
            code.Argument = ira;
            p.TileChange.Code.Add(code);

            code = new HookCode();
            code.OpCode = OpCodes.Ldarg_0;
            p.TileChange.Code.Add(code); code = new HookCode();

            code.OpCode = OpCodes.Ldfld;
            fra = new FieldRefArg();
            fra.TypeName = "Terraria.messageBuffer";
            fra.FieldName = "whoAmI";
            code.Argument = fra;
            p.TileChange.Code.Add(code);

            code = new HookCode();
            code.OpCode = OpCodes.Ldloc_S;
            vra = new VariableRefArg();
            vra.TypeName = "Terraria.messageBuffer";
            vra.MethodName = "GetData";
            vra.ArgumentTypes = new[] { "System.Int32", "System.Int32" };
            vra.VariableIndex = 20;
            code.Argument = vra;
            p.TileChange.Code.Add(code);

            code = new HookCode();
            code.OpCode = OpCodes.Ldloc_S;
            vra = new VariableRefArg();
            vra.TypeName = "Terraria.messageBuffer";
            vra.MethodName = "GetData";
            vra.ArgumentTypes = new[] { "System.Int32", "System.Int32" };
            vra.VariableIndex = 21;
            code.Argument = vra;
            p.TileChange.Code.Add(code);

            code = new HookCode();
            code.OpCode = OpCodes.Ldc_I4_1;
            p.TileChange.Code.Add(code);

            code = new HookCode();
            code.OpCode = OpCodes.Call;
            mra = new MethodRefArg();
            mra.TypeName = "Terraria.NetMessage";
            mra.MethodName = "SendTileSquare";
            mra.ArgumentTypes = new[] { "System.Int32", "System.Int32", "System.Int32", "System.Int32" };
            code.Argument = mra;
            p.TileChange.Code.Add(code);

            code = new HookCode();
            code.OpCode = OpCodes.Ret;
            p.TileChange.Code.Add(code);

            return p;
        }

    }
}
