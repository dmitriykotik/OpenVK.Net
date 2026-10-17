using IniParser;
using IniParser.Model;

namespace OpenVK.Cmd
{
    internal class Settings
    {
        private readonly string m_FileName;
        private readonly FileIniDataParser m_Parser;

        public Settings(string fileName = "settings.ini") 
        { 
            m_FileName = fileName;

            if (!File.Exists(m_FileName))
                File.WriteAllText(m_FileName, GenSampleIniSettings());

            m_Parser = new FileIniDataParser();
        }

        public void SetValue(string section, string variable, string value)
        {
            IniData data = m_Parser.ReadFile(m_FileName);
            data[section][variable] = value;
            m_Parser.WriteFile(m_FileName, data);
        }

        public string GetValue(string section, string variable)
        {
            IniData data = m_Parser.ReadFile(m_FileName);
            return data[section][variable];
        }

        private static string GenSampleIniSettings()
        {
            return "[server]\ninstance_api_url = \n[user]\naccess_token = \nexpires_in = \nuser_id = \n";
        }
    }
}
