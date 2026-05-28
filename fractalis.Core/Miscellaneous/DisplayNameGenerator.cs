using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace fractalis.Core.Miscellaneous
{
    public static class DisplayNameGenerator
    {
        private static readonly string[] _adjectives =
        [
            "Amber", "Bold", "Calm", "Daring", "Eager",
            "Fierce", "Golden", "Hidden", "Icy", "Jolly",
            "Kind", "Lively", "Mystic", "Noble", "Odd",
            "Peppy", "Quick", "Rustic", "Silent", "Tidy",
            "Urban", "Vivid", "Wild", "Xenic", "Zesty",
            "Ancient", "Blazing", "Crimson", "Dusty", "Electric",
            "Frosty", "Gloomy", "Hollow", "Iron", "Jade",
            "Keen", "Lunar", "Mossy", "Neon", "Obsidian",
            "Primal", "Quantum", "Rogue", "Stormy", "Twilight",
            "Umber", "Velvet", "Wandering", "Xenial", "Zealous"
        ];

        private static readonly string[] _nouns =
        [
            "Badger", "Cobra", "Dingo", "Eagle", "Falcon",
            "Gecko", "Heron", "Ibis", "Jackal", "Koala",
            "Lemur", "Mamba", "Newt", "Otter", "Panda",
            "Quail", "Raven", "Stoat", "Tapir", "Urubu",
            "Viper", "Walrus", "Xerus", "Yak", "Zebra",
            "Axolotl", "Bison", "Capybara", "Dugong", "Ermine",
            "Fennec", "Gharial", "Hyena", "Impala", "Jaguar",
            "Kestrel", "Lynx", "Margay", "Numbat", "Ocelot",
            "Pangolin", "Quokka", "Seal", "Serval", "Takin",
            "Urial", "Vicuna", "Wombat", "Xenops", "Zorilla"
        ];

        /// <summary>
        /// Generates a random display name in the form "AdjectiveNoun123".
        /// </summary>
        public static string Generate()
        {
            var adjective = _adjectives[Random.Shared.Next(_adjectives.Length)];
            var noun = _nouns[Random.Shared.Next(_nouns.Length)];
            var suffix = Random.Shared.Next(100, 999);

            return $"{adjective}{noun}{suffix}";
        }
    }
}
