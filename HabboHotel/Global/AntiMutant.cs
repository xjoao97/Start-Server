#region

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Xml.Linq;

#endregion

namespace Oblivion.HabboHotel.Global
{
    public class AntiMutant
    {
        private readonly Dictionary<string, Dictionary<string, Figure>> _parts;

        public AntiMutant()
        {
            _parts = new Dictionary<string, Dictionary<string, Figure>>();
            Init();
        }

        public void Init()
        {
            if (_parts.Count > 0)
                _parts.Clear();

            try
            {
                var Doc = XDocument.Load(Path.Combine(Application.StartupPath, @"extra/figuredata.xml"));

                var data = from item in Doc.Descendants("sets")
                    from tItem in Doc.Descendants("settype")
                    select new {Part = tItem.Elements("set"), Type = tItem.Attribute("type")};
                foreach (var item in data.ToList())
                foreach (var part in item.Part.ToList())
                {
                    if (part == null)
                        return;
                    var PartName = item.Type.Value;
                    if (!_parts.ContainsKey(PartName))
                        _parts.Add(PartName, new Dictionary<string, Figure>());

                    var toAddFigure = new Figure(PartName, part.Attribute("id").Value,
                        part.Attribute("gender").Value, part.Attribute("colorable").Value);

                    if (!_parts[PartName].ContainsKey(part.Attribute("id").Value))
                        _parts[PartName].Add(part.Attribute("id").Value, toAddFigure);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                //Was the file found?
            }
        }

        public string RunLook(string Look)
        {
            var toReturnFigureParts = new List<string>();
            var fParts = new List<string>();
            string[] requiredParts = {"hd", "ch"};
            var flagForDefault = false;

            var FigureParts = Look.Split('.');
            var genderLook = GetLookGender(Look);

            foreach (var Part in FigureParts.ToList())
            {
                var newPart = Part;
                var tPart = Part.Split('-');
                if (tPart.Length < 2)
                {
                    flagForDefault = true;
                    continue;
                }

                var partName = tPart[0];
                var partId = tPart[1];

                if (!_parts.ContainsKey(partName) || !_parts[partName].ContainsKey(partId) ||
                    genderLook != "U" && _parts[partName][partId].Gender != "U" &&
                    _parts[partName][partId].Gender != genderLook)
                    if (partName == "hd" && partId == "99999")
                    {
                        if (tPart.Length == 2)
                            newPart = SetDefault(partName, genderLook);
                    }
                    else
                    {
                        newPart = SetDefault(partName, genderLook);
                    }

                if (!fParts.Contains(partName)) fParts.Add(partName);
                if (!toReturnFigureParts.Contains(newPart)) toReturnFigureParts.Add(newPart);
            }

            if (flagForDefault)
            {
                toReturnFigureParts.Clear();
                toReturnFigureParts.AddRange(
                    "sh-3338-93.ea-1406-62.hr-831-49.ha-3331-92.hd-180-7.ch-3334-93-1408.lg-3337-92.ca-1813-62".Split(
                        '.'));
            }

            foreach (
                var requiredPart in
                requiredParts.Where(
                    requiredPart =>
                        !fParts.Contains(requiredPart) &&
                        !toReturnFigureParts.Contains(SetDefault(requiredPart, genderLook))))
                toReturnFigureParts.Add(SetDefault(requiredPart, genderLook));

            return string.Join(".", toReturnFigureParts);
        }

        private string GetLookGender(string Look)
        {
            var FigureParts = Look.Split('.');

            foreach (var Part in FigureParts.ToList())
            {
                var tPart = Part.Split('-');
                if (tPart.Length < 2)
                    continue;

                var partName = tPart[0];
                var partId = tPart[1];

                return _parts.ContainsKey(partName) && _parts[partName].ContainsKey(partId)
                    ? _parts[partName][partId].Gender
                    : "U";
            }
            return "U";
        }

        private string SetDefault(string partName, string Gender)
        {
            var partId = "0";
            if (_parts.ContainsKey(partName))
            {
                var part = _parts[partName].FirstOrDefault(x => x.Value.Gender == Gender || Gender == "U");
                partId = part.Equals(default(KeyValuePair<string, Figure>)) ? "0" : part.Key;
            }
            return partName + "-" + partId + "-1";
        }
    }

    internal class Figure
    {
        private string Colorable;
        public string Gender;
        private string Part;
        private string PartId;

        public Figure(string Part, string PartId, string Gender, string Colorable)
        {
            this.Part = Part;
            this.PartId = PartId;
            this.Gender = Gender;
            this.Colorable = Colorable;
        }
    }
}