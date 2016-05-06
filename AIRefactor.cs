using Terraria.ModLoader;

namespace AIRefactor {
    public class AIRefactor : Mod {
        public AIRefactor() {
            Properties = new ModProperties() {
                Autoload = true,
                AutoloadGores = true,
                AutoloadSounds = true
            };
        }
    }
}
