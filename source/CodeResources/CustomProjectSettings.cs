using Godot;

namespace BrickModelGame.source.CodeResources
{
    public static class CustomProjectSettings
    {
        private static bool m_needToSave = false;
        private const string RootPath = "brick_model_game";
        public const string DebugTurretAiming = RootPath + "/debug_turret_aiming";

        static CustomProjectSettings()
        {
            if (!ProjectSettings.HasSetting(DebugTurretAiming))
                RegisterSetting(DebugTurretAiming, false);

            if (m_needToSave)
                ProjectSettings.Save();
        }

        // TODO make this a tempalte if you get it working
        private static void RegisterSetting(string path, bool value)
        {
            m_needToSave = true;
            ProjectSettings.SetSetting(path, value);
            ProjectSettings.SetAsBasic(DebugTurretAiming, true);
        }

        // TODO: Maybe remove this?
        public static bool GetDebugTurretAiming()
        {
            return (bool)ProjectSettings.GetSetting(DebugTurretAiming);
        }
    }
}
