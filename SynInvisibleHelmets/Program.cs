using System;
using System.IO;
using Mutagen.Bethesda;
using Mutagen.Bethesda.Synthesis;
using Mutagen.Bethesda.Skyrim;
using Newtonsoft.Json.Linq;
using SynInvisibleHelmets.Types;
namespace SynInvisibleHelmets
{
    public class Program
    {
        public static int Main(string[] args)
        {
            return SynthesisPipeline.Instance.Patch<ISkyrimMod, ISkyrimModGetter>(
                args: args,
                patcher: RunPatch,
                userPreferences: new UserPreferences()
                {
                    ActionsForEmptyArgs = new RunDefaultPatcher()
                    {
                        IdentifyingModKey = "SynInvisibleHelmets.esp",
                        TargetRelease = GameRelease.SkyrimSE,
                        BlockAutomaticExit = true,
                    }
                });
        }

        public static void RunPatch(SynthesisState<ISkyrimMod, ISkyrimModGetter> state)
        {
            var settings = JObject.Parse(File.ReadAllText(Path.Combine(state.ExtraSettingsDataPath, "settings.json"))).ToObject<Settings>();
            foreach(var armor in state.LoadOrder.PriorityOrder.Armor().WinningOverrides()) {
                if(armor.BodyTemplate != null && armor.BodyTemplate.FirstPersonFlags.HasFlag(BipedObjectFlag.Hair)) {
                    var na = state.PatchMod.Armors.GetOrAddAsOverride(armor);
                    Console.WriteLine($"Patching {na.Name?.String}");
                    if(na.BodyTemplate!=null) {
                        na.BodyTemplate.FirstPersonFlags &= ~BipedObjectFlag.Hair;
                        na.BodyTemplate.FirstPersonFlags |= (BipedObjectFlag)(1<<settings.slotToUse);
                    }
                }
            }
        }
    }
}
