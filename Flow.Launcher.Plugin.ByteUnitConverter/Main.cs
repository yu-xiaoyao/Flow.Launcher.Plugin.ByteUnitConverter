using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Documents;
using Flow.Launcher.Plugin;

namespace Flow.Launcher.Plugin.ByteUnitConverter
{
    public class ByteUnitConverter : IPlugin, IPluginI18n
    {
        public const string IconPath = "Images\\byte-unit-converter.png";
        private PluginInitContext _context;

        public void Init(PluginInitContext context)
        {
            _context = context;
        }

        public List<Result> Query(Query query)
        {
            var firstSearch = query.FirstSearch.Trim();
            var secondSearch = query.SecondSearch.Trim();

            if (string.IsNullOrEmpty(firstSearch))
            {
                return new List<Result>
                {
                    new()
                    {
                        IcoPath = IconPath,
                        SubTitle = _context.API.GetTranslation("default_input_sub_title"),
                        Action = _ =>
                        {
                            _context.API.ChangeQuery($"{query.ActionKeyword} ");
                            return false;
                        },
                        AutoCompleteText = $"{query.ActionKeyword} "
                    }
                };
            }


            var byteUnit = DetectByteUnit(secondSearch);
            var validNum = decimal.TryParse(firstSearch, out var size);
            if (validNum)
            {
                var intSize = Math.Truncate(size);
                decimal originSize;
                if (size - intSize == 0m)
                {
                    originSize = intSize;
                    // int number
                    if (byteUnit == null)
                    {
                        return GetUnitsList(query.ActionKeyword, $"{originSize}", ByteUnit.bit, secondSearch);
                    }
                }
                else
                {
                    originSize = size;
                    switch (byteUnit)
                    {
                        // float number
                        case null:
                            return GetUnitsList(query.ActionKeyword, $"{originSize}", ByteUnit.KB, secondSearch);
                        case <= ByteUnit.bytes:
                            // 小数不支持小单位.
                            return GetErrorResults(query.ActionKeyword, $"{originSize}");
                    }
                }

                return GetConvertResults(query.ActionKeyword, originSize, (ByteUnit)byteUnit);
            }

            #region ERROR

            /*
            var parse = ulong.TryParse(firstSearch, out var smallSize);
            if (parse)
            {
                if (byteUnit == null)
                {
                    return GetUnitsList(query.ActionKeyword, $"{smallSize}", ByteUnit.bit, secondSearch);
                }

                return GetConvertResults(query.ActionKeyword, smallSize, (ByteUnit)byteUnit);
            }

            parse = double.TryParse(firstSearch, out var bigSize);
            if (parse)
            {
                if (byteUnit == null)
                {
                    return GetUnitsList(query.ActionKeyword, $"{bigSize}", ByteUnit.KB, secondSearch);
                }
            }
            */

            #endregion

            return GetErrorResults(query.ActionKeyword);
        }

        private ByteUnit? DetectByteUnit(string input)
        {
            if (string.IsNullOrEmpty(input)) return null;
            var units = Enum.GetValues<ByteUnit>();
            foreach (var byteUnit in units)
            {
                if (byteUnit.ToString().Equals(input))
                {
                    return byteUnit;
                }
            }

            return null;
        }

        private List<Result> GetConvertResults(string actionKeyword, decimal from, ByteUnit fromUnit)
        {
            if (from <= 0m)
                return GetErrorResults(actionKeyword);

            var units = Enum.GetNames<ByteUnit>();

            var result = new List<Result>();

            var index = (int)fromUnit;

            var origin = from;
            var scaleIndex = 1;
            for (var i = index + 1; i < units.Length; i++)
            {
                if (i - 1 == 0)
                {
                    origin /= 8;
                    var convertResult = origin;
                    result.Add(new Result
                    {
                        IcoPath = IconPath,
                        Title = $"{convertResult}",
                        SubTitle = $"{from} {fromUnit} = {convertResult} {units[i]}",
                        CopyText = $"{convertResult}",
                        Action = _ =>
                        {
                            _context.API.CopyToClipboard($"{convertResult}", false, false);
                            return true;
                        }
                    });
                }
                else
                {
                    var curScale = Convert.ToDecimal(Math.Pow(1024, scaleIndex));
                    scaleIndex++;
                    var convertResult = origin / curScale;
                    result.Add(new Result
                    {
                        IcoPath = IconPath,
                        Title = $"{convertResult}",
                        SubTitle = $"{from} {fromUnit} = {convertResult} {units[i]}",
                        CopyText = $"{convertResult}",
                        Action = _ =>
                        {
                            _context.API.CopyToClipboard($"{convertResult}", false, false);
                            return true;
                        }
                    });
                    if (convertResult < 1.0m)
                        break;
                }
            }

            origin = from;
            for (var i = index - 1; i >= 0; i--)
            {
                if (i == 0)
                {
                    origin *= 8;
                }
                else
                {
                    origin *= 1024;
                }

                var convertResult = origin;
                result.Add(new Result
                {
                    IcoPath = IconPath,
                    Title = $"{convertResult}",
                    SubTitle = $"{from} {fromUnit} = {convertResult} {units[i]}",
                    CopyText = $"{convertResult}",
                    Action = _ =>
                    {
                        _context.API.CopyToClipboard($"{convertResult}", false, false);
                        return true;
                    }
                });
            }

            return result;
        }

        private List<Result> GetUnitsList(string actionKeyword,
            string inputValue,
            ByteUnit startIndex = ByteUnit.bit,
            string inputKey = null)
        {
            var units = Enum.GetValues<ByteUnit>();
            var result = new List<Result>();

            for (var i = (int)startIndex; i < units.Length; i++)
            {
                var unit = units[i];

                var unitText = unit.ToString();
                if (!string.IsNullOrEmpty(inputKey))
                {
                    if (unitText.Equals(inputKey))
                        continue;

                    if (!unitText.StartsWith(inputKey))
                        continue;
                }

                result.Add(new Result
                {
                    IcoPath = IconPath,
                    Title = unitText,
                    SubTitle = unitText,
                    AutoCompleteText = $"{actionKeyword} {inputValue} {unitText}",
                    Action = _ =>
                    {
                        _context.API.ChangeQuery($"{actionKeyword} {inputValue} {unitText}");
                        return false;
                    }
                });
            }

            return result.Any() ? result : GetErrorResults(actionKeyword, inputValue);
        }


        private List<Result> GetErrorResults(string actionKeyword, string appendValue = "")
        {
            if (!string.IsNullOrEmpty(appendValue))
            {
                appendValue += " ";
            }

            // input error
            return new List<Result>
            {
                new()
                {
                    IcoPath = IconPath,
                    SubTitle = _context.API.GetTranslation("input_error"),
                    AutoCompleteText = $"{actionKeyword} {appendValue}",
                    Action = _ =>
                    {
                        _context.API.ChangeQuery($"{actionKeyword} {appendValue}");
                        return false;
                    }
                }
            };
        }

        public string GetTranslatedPluginTitle()
        {
            return _context.API.GetTranslation("plugin_title");
        }

        public string GetTranslatedPluginDescription()
        {
            return _context.API.GetTranslation("plugin_desp");
        }
    }
}