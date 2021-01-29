using System.Collections;
using System.Collections.Generic;
using System.Management.Instrumentation;
using MoreMountains.TopDownEngine;
using UnityEngine;

namespace SP
{
    public enum ElementalReactionType
    {
        Burn,
        Soaked,
        Exhausted
    }

    public interface IElementalReaction
    {
        void MatchAndCreate();
    }

    public class SPElementalReactionExhusted : IElementalReaction
    {
        public void MatchAndCreate() { }
    }

    public class SPElementalReactionSoaked : IElementalReaction
    {
        public void MatchAndCreate() { }
    }

    public class SPElementalReactionBurn : IElementalReaction
    {
        public void MatchAndCreate() { }
    }

    public static class SPElementalReactionFactory
    {
        private static List<ReactionCombination> PredefinedCombinations;
        struct ReactionCombination
        {
            private string Name;
            private ElementalTypes [] CombiElems;
            public ReactionCombination(string name, ElementalTypes [] combiElems)
            {
                Name = name;
                CombiElems = combiElems;
            }
        }
        public static List<IElementalReaction> LookUpPossibleReaction(ElementalTypes [] candidateElem)
        {
            List<IElementalReaction> generated = new List<IElementalReaction>();
            generated.Add(new SPElementalReactionBurn());
            generated.Add(new SPElementalReactionSoaked());
            generated.Add(new SPElementalReactionExhusted());
            
            return generated;
        }

        public static void CreatePredefinedCombinaton()
        {
            PredefinedCombinations = new List<ReactionCombination>();
            PredefinedCombinations.Add(new ReactionCombination("burn", new []{ ElementalTypes.FIRE, ElementalTypes.FIRE } ));
            PredefinedCombinations.Add(new ReactionCombination("soaked", new[] { ElementalTypes.WATER, ElementalTypes.WATER }));
            PredefinedCombinations.Add(new ReactionCombination("exhuseted", new[] { ElementalTypes.FIRE, ElementalTypes.WATER }));
            PredefinedCombinations.Add(new ReactionCombination("burn", new[] { ElementalTypes.FIRE, ElementalTypes.WATER, ElementalTypes.WATER }));
            PredefinedCombinations.Add(new ReactionCombination("burn", new[] { ElementalTypes.FIRE, ElementalTypes.FIRE }));

        }

        public static void Affect()
        {
            
        }

    }
}

