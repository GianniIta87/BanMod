using System;

namespace BanMod
{
    public class IntegerOptionItem : OptionItem
    {
        public IntegerValueRule Rule;

        public IntegerOptionItem(int id, string name, int defaultValue, TabGroup tab, bool isSingleValue, IntegerValueRule rule)
        : base(id, name, rule.GetNearestIndex(defaultValue), tab, isSingleValue)
        {
            Rule = rule;
        }
        public static IntegerOptionItem Create(
            int id, string name, IntegerValueRule rule, int defaultValue, TabGroup tab, bool isSingleValue
        )
        {
            return new IntegerOptionItem(
                id, name, defaultValue, tab, isSingleValue, rule
            );
        }
        public static IntegerOptionItem Create(
            int id, Enum name, IntegerValueRule rule, int defaultValue, TabGroup tab, bool isSingleValue
        )
        {
            return new IntegerOptionItem(
                id, name.ToString(), defaultValue, tab, isSingleValue, rule
            );
        }

        public override int GetInt() => Rule.GetValueByIndex(CurrentValue);
        public override float GetFloat() => Rule.GetValueByIndex(CurrentValue);
        public override string GetString()
        {
            return ApplyFormat(Rule.GetValueByIndex(CurrentValue).ToString());
        }
        public override int GetValue()
            => Rule.RepeatIndex(base.GetValue());

        public override void SetValue(int value, bool doSync = true)
        {
            base.SetValue(Rule.RepeatIndex(value), doSync);
        }
    }
}