using System;
using Newtonsoft.Json;
using UnityEngine;

namespace DigAndWonder
{
    [CreateAssetMenu(menuName = "Dig & Wonder/Data/Economy", fileName = "Economy")]
    public sealed class EconomyDefinition : ScriptableObject
    {
        public TextAsset balanceTable;

        public EconomyRules ToRuntime()
        {
            if (balanceTable == null) throw new InvalidOperationException("Tabela de economia ausente.");
            EconomyRules rules = JsonConvert.DeserializeObject<EconomyRules>(balanceTable.text);
            if (rules == null) throw new InvalidOperationException("Tabela de economia vazia.");
            rules.Validate();
            return rules;
        }
    }
}
