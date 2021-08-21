using System;
using System.Collections.Generic;

namespace BadgerBoss
{
    public static class RuleFactory
    {
        public static Rule GetNewRule()
        {
            return new Rule[] { TurnOrderRule.Random(), PhraseRule.Random(), SkipRule.Random(), InvalidRule.Random() }.PickRandom();
        }

        public static Rule GetDefaultRules()
        {
            return new DefaultRules();
        }
    }
}