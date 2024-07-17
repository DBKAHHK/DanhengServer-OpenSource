using EggLink.DanhengServer.Enums.Mission;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EggLink.DanhengServer.Data.Excel
{
    [ResourceEntity("HeartDialScript.json")]
    public class HeartDialScriptExcel : ExcelResource
    {
        public int ScriptID { get; set; }
        [JsonConverter(typeof(StringEnumConverter))]
        public HeartDialEmoTypeEnum DefaultEmoType { get; set; } = HeartDialEmoTypeEnum.Peace;

        public override int GetId()
        {
            return ScriptID;
        }

        public override void Loaded()
        {
            GameData.HeartDialScriptData[ScriptID] = this;
        }
    }
}
