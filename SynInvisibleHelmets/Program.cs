using System;
using System.IO;
using System.Threading.Tasks;
using Mutagen.Bethesda;
using Mutagen.Bethesda.Synthesis;
using Mutagen.Bethesda.Skyrim;
using Newtonsoft.Json.Linq;
using SynInvisibleHelmets.Types;
namespace SynInvisibleHelmets
{
    public class Program
    {
        public static async Task<int> Main(string[] args)
        {
            return await SynthesisPipeline.Instance.AddPatch<ISkyrimMod, ISkyrimModGetter>(RunPatch).Run(args, new RunPreferences()
            {
                ActionsForEmptyArgs = new RunDefaultPatcher()
                {
                    IdentifyingModKey = "SynInvisibleHelmets.esp",
                    TargetRelease = GameRelease.SkyrimSE
                }
            });
        }

        public static void RunPatch(IPatcherState<ISkyrimMod, ISkyrimModGetter> state)
        {
            var settings = JObject.Parse(File.ReadAllText(Path.Combine(state.ExtraSettingsDataPath, "settings.json"))).ToObject<Settings>();
            foreach (var armor in state.LoadOrder.PriorityOrder.Armor().WinningOverrides())
            {
                if (armor.BodyTemplate != null && armor.BodyTemplate.FirstPersonFlags.HasFlag(BipedObjectFlag.Hair))
                {
                    var na = state.PatchMod.Armors.GetOrAddAsOverride(armor);
                    Console.WriteLine($"Patching {na.Name?.String}");
                    if (na.BodyTemplate != null)
                    {
                        na.BodyTemplate.FirstPersonFlags &= ~BipedObjectFlag.Hair;
                        na.BodyTemplate.FirstPersonFlags |= (BipedObjectFlag)(1 << settings.slotToUse);
                    }
                }
            }
        }
    }
}
