using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class ProtocolData
{
    public int K { get; set; }
    public int F { get; set; }
    public int Q { get; set; }
    public double B { get; set; }
    public int A { get; set; }
    public double R { get; set; }
    public DateTime T { get; set; }

    public static ProtocolData Deserialize(string input)
    {
        var data = new ProtocolData();
        var matches = Regex.Matches(input, @"(\w):([^;{}]+)");

        byte b1 = 0, b2 = 0, r1 = 0, r2 = 0, r3 = 0;

        foreach (Match match in matches)
        {
            string key = match.Groups[1].Value;
            string value = match.Groups[2].Value.Trim();

            try
            {
                switch (key)
                {
                    case "K": data.K = int.Parse(value); break;
                    case "F": data.F = int.Parse(value); break;
                    case "Q": data.Q = int.Parse(value); break;
                    case "B":
                        if (value.Length == 4)
                        {
                            b1 = Convert.ToByte(value.Substring(0, 2), 16);
                            b2 = Convert.ToByte(value.Substring(2, 2), 16);
                            data.B = ((b1 & 0xF0) >> 4) * 10 + (b2 & 0x0F);
                        }
                        break;
                    case "A": data.A = int.Parse(value); break;
                    case "R":
                        if (value.Length == 6)
                        {
                            r1 = Convert.ToByte(value.Substring(0, 2), 16);
                            r2 = Convert.ToByte(value.Substring(2, 2), 16);
                            r3 = Convert.ToByte(value.Substring(4, 2), 16);
                            data.R = Calculations.BacktrancAVR(r1, r2, r3);
                        }
                        break;
                    case "T":
                        data.T = DateTime.ParseExact(value, "dd/MM/yy,HH:mm:sszz", null);
                        break;
                }
            }
            catch (FormatException ex)
            {
                Console.WriteLine($"Ошибка преобразования значения '{value}' в {key}: {ex.Message}");
            }
            catch (OverflowException ex)
            {
                Console.WriteLine($"Ошибка переполнения при обработке {key}: {value}, {ex.Message}");
            }
        }
        return data;
    }

    public static ProtocolData CreateRandom(int k, int f)
    {
        var rand = new Random();
        var data = new ProtocolData
        {
            K = k,
            F = f,
            Q = rand.Next(1, 100),
            B = ((rand.Next(0, 16) << 4) * 10) + rand.Next(0, 16),
            A = rand.Next(1, 100),
            R = Calculations.BacktrancAVR((byte)rand.Next(0, 256), (byte)rand.Next(0, 256), (byte)rand.Next(0, 256)),
            T = DateTime.Now
        };
        return data;
    }

    public override string ToString()
    {
        return $"{F};K:{K};F:{F};Q:{Q};B:{(int)(B / 10):X2}{(int)(B % 10):X2};A:{A};R:{((int)R >> 16):X2}{(((int)R >> 8) & 0xFF):X2}{((int)R & 0xFF):X2};T:{T:dd/MM/yy,HH:mm:ss};";
    }
}

public static class Calculations
{
    public static double BacktrancAVR(byte b1, byte b2, byte b3)
    {
        return (b1 << 16) | (b2 << 8) | b3;
    }
}
