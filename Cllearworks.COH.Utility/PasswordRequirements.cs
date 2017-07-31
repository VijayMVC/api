using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Cllearworks.COH.Utility
{
    public class PasswordRequirements
    {
        public PasswordRequirement Requirements { get; set; }
        public int? MinLength { get; set; }
        public int? MinRequirements { get; set; }

        public bool Validate(string password)
        {
            var reqStat = new Dictionary<PasswordRequirement, bool>();

            if (MinLength.HasValue)
                if (password.Length < MinLength.Value)
                    return false;

            if (Requirements.HasFlag(PasswordRequirement.Lower))
                reqStat.Add(PasswordRequirement.Lower, Regex.IsMatch(password, "^(?=.*[a-z])"));
            if (Requirements.HasFlag(PasswordRequirement.Upper))
                reqStat.Add(PasswordRequirement.Upper, Regex.IsMatch(password, "^(?=.*[A-Z])"));
            if (Requirements.HasFlag(PasswordRequirement.Numeric))
                reqStat.Add(PasswordRequirement.Numeric, Regex.IsMatch(password, "^(?=.*[0-9])"));
            if (Requirements.HasFlag(PasswordRequirement.SpecialCharacter))
                reqStat.Add(PasswordRequirement.SpecialCharacter, Regex.IsMatch(password, "^(?=.*[^a-zA-Z0-9])"));

            if (MinRequirements.HasValue)
                return reqStat.Count(x => x.Value) >= MinRequirements.Value;
            return reqStat.All(x => x.Value);
        }
    }

    [Flags]
    [JsonConverter(typeof(StringEnumConverter))]
    public enum PasswordRequirement
    {
        Upper = 1,
        Lower = 2,
        Numeric = 4,
        SpecialCharacter = 8
    }
}
