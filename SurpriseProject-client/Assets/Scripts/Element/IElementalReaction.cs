using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SP
{
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

    public class SPElementalReactionFactory
    {

        public void LookUpPossibleReaction(SPElemental elemental)
        {

        }
    }
}

