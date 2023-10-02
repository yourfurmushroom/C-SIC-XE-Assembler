using System;
using System.Data;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection.Metadata.Ecma335;

namespace ConsoleApp1
{
    internal class Program
    {
        static Dictionary<string, string> opcode = new Dictionary<string, string>()
        {
            {"ADD","0x18" },
            {"ADDF","0x58" },
            {"ADDR","0x90" },
            {"AND","0x40" },
            {"CLEAR","0xB4" },
            {"COMP","0x28" },
            {"COMPF","0x88" },
            {"COMPR","0xA0" },
            {"DIV","0x24" },
            {"DIVF","0x64" },
            {"DIVR","0x9C" },
            {"FIX","0xC4" },
            {"FLOAT","0xC0" },
            {"HIO","0xF4" },
            {"J","0x3C" },
            {"JEQ","0x30" },
            {"JGT","0x34" },
            {"JLT","0x38" },
            {"JSUB","0x48" },
            {"LDA","0x00" },
            {"LDB","0x68" },
            {"LDCH","0x50" },
            {"LDF","0x70" },
            {"LDL","0x08" },
            {"LDS","0x6C" },
            {"LDT","0x74" },
            {"LDX","0x04" },
            {"LPS","0xD0" },
            {"MUL","0x20" },
            {"MULF","0x60" },
            {"MULR","0x98" },
            {"NORM","0xC8" },
            {"OR","0x44" },
            {"RD","0xD8" },
            {"RMO","0xAC" },
            {"RSUB","0x4C" },
            {"SHIFTL","0xA4" },
            {"SHIFTR","0xA8" },
            {"SIO","0xF0" },
            {"SSK","0xEC" },
            {"STA","0x0C" },
            {"STB","0x78" },
            {"STCH","0x54" },
            {"STF","0x80" },
            {"STI","0xD4" },
            {"STL","0x14" },
            {"STS","0x7C" },
            {"STSW","0xE8" },
            {"STT","0x84" },
            {"STX","0x10" },
            {"SUB","0x1C" },
            {"SUBF","0x5C" },
            {"SUBR","0x94" },
            {"SVC","0xB0" },
            {"TD","0xE0" },
            {"TIO","0xF8" },
            {"TIX","0x2C" },
            {"TIXR","0xB8" },
            {"WD","0xDC" }
        };
        static Dictionary<char, int> hexTable = new Dictionary<char, int>()
        {
            {'0',0 },
            {'1',1 },
            {'2',2 },
            {'3',3 },
            {'4',4 },
            {'5',5 },
            {'6',6 },
            {'7',7 },
            {'8',8 },
            {'9',9 },
            {'A',10 },
            {'B',11 },
            {'C',12},
            {'D',13 },
            {'E',14 },
            {'F',15 }
        };
        static Dictionary<string, string> register = new Dictionary<string, string>()
        {
            {"A","0"},
            {"X","1"},
            {"T","5"},
            {"S","4"},

            {"B","3"}
        };
        static void Main(string[] args)
        {

            Content content = new Content();
            content.Initialize();
            try
            {
                content.CalculateLOC();
                content.CalculateOBJCode();
                StreamWriter sw = new StreamWriter("TestFile1.TXT");
                WriteSymtab(content, sw);
                WriteLocAndObj(content, sw);
                sw.Close();

            }
            catch (Exception ex)
            {
                Console.WriteLine("your code cannot be done,please check your code");
            }

        }

        private static void WriteLocAndObj(Content content, StreamWriter sw)
        {
            sw.WriteLine("===LOC===");
            for (int i = 0; i < content.LOC.Count; i++)
            {
                if (content.LOC[i] != "")
                {
                    sw.WriteLine(i.ToString().PadRight(3, ' ') + " LOC=" + content.LOC[i].PadRight(7, ' ') + " " + content.OBJCODE[i]);
                    Console.WriteLine(i.ToString().PadRight(3, ' ') + " LOC=" + content.LOC[i].PadRight(7, ' ') + " " + content.OBJCODE[i]);
                }
                else
                {
                    sw.WriteLine(i.ToString().PadRight(3, ' ') + " " + content.OBJCODE[i]);
                    Console.WriteLine(i.ToString().PadRight(3, ' ') + " " + content.OBJCODE[i]);
                }
            }
        }

        private static void WriteSymtab(Content content, StreamWriter sw)
        {
            sw.WriteLine("===SYMTAB===");
            foreach (var item in content.SYMTAB)
            {
                sw.WriteLine(item);
            }
        }

        static List<List<string>> ReadTxt(string path)
        {
            try
            {
                StreamReader sr = new StreamReader(path);
                string line = "";
                List<List<string>> returnOP = new List<List<string>>();
                while ((line = sr.ReadLine()) != null)
                {
                    List<string> list = new List<string>();
                    string[] split = line.Split(' ');
                    foreach (string s in split)
                    {
                        if (s != "") list.Add(s.ToUpper());
                        else continue;
                    }
                    returnOP.Add(list);
                }
                sr.Close();
                return returnOP;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return null;
            }
        }
        
        public class Content
        {
            string txtPath;
            List<List<string>> operation;
            public List<string> LOC;
            public List<string> OBJCODE;
            public List<string> SYMTAB;
            public Dictionary<string, int> SymtabPair;
            public int baseLoc = 0;

            public void Initialize()
            {
                txtPath= "C:\\Users\\zihui\\Downloads\\1.txt";
                operation = ReadTxt(txtPath);
                LOC=new List<string>();
                OBJCODE = new List<string>();
                SYMTAB = new List<string>();
                SymtabPair=new Dictionary<string, int>();
            }

            public void CalculateLOC()
            {
                string startingAddress = operation[0][operation[0].Count - 1];
                LOC.Add(startingAddress.PadLeft(4,'0'));
                int startingLoc = Convert.ToInt32(startingAddress);
                string temp = $"{operation[0][0].PadRight(9, ' ')} {startingLoc.ToString("X4").PadLeft(4, '0')}";
                SYMTAB.Add(temp);
                SymtabPair[operation[0][0]] = startingLoc;
                for (int i = 1; i < operation.Count; i++)
                {
                    if (operation[i].Count == 3)
                    {
                        if (operation[i][0] != ".")
                        {
                            temp = $"{operation[i][0].PadRight(9, ' ')} {startingLoc.ToString("X4").PadLeft(4, '0')}";
                            SYMTAB.Add(temp);
                        }
                        SymtabPair[operation[i][0]] = startingLoc;
                        if (  operation[i][1] == "RSUB" )
                        {
                            LOC.Add(startingLoc.ToString("X4").PadLeft(4, '0'));
                            startingLoc += 3;
                        }
                        else if (operation[i][0] == "." )
                        {
                            LOC.Add("");
                        }
                        else if (operation[i][1] == "BASE")
                        {
                            LOC.Add("");
                        }
                        else if (operation[i][1] == "EQU")  
                        {
                            if (operation[i][2] == "*")
                            {
                                LOC.Add(startingLoc.ToString("X4").PadLeft(4, '0'));
                            }
                            else
                            {
                                string[] spl = operation[i][2].Split('-');
                                string index0 = spl[0];
                                string index1 = spl[1];
                                LOC.Add((SymtabPair[index0] - SymtabPair[index1]).ToString("X4"));
                                SymtabPair[operation[i][0]] = (SymtabPair[index0] - SymtabPair[index1]);
                            }
                        }
                        else if (operation[i][1] == "RESB")
                        {
                            int offset = Convert.ToInt32(operation[i][2]);
                            LOC.Add(startingLoc.ToString("X4").PadLeft(4, '0'));
                            startingLoc += offset;
                        }
                        else if(operation[i][1] == "RESW")
                        {
                            int offset = Convert.ToInt32(operation[i][2])*3;
                            LOC.Add(startingLoc.ToString("X4").PadLeft(4, '0'));
                            startingLoc += offset;
                        }
                        else if(operation[i][1] == "BYTE")
                        {
                            if (operation[i][2][0]=='C')
                            {
                                int addCount= operation[i][2].Length-3;
                                LOC.Add(startingLoc.ToString("X4").PadLeft(4, '0'));
                                startingLoc += addCount;
                            }
                            else
                            {
                                int offset = operation[i][2].Length - 3;
                                LOC.Add(startingLoc.ToString("X4").PadLeft(4, '0'));
                                startingLoc += offset / 2;
                            }
                        }
                        else if (operation[i][1] == "CLEAR")
                        {
                            LOC.Add(startingLoc.ToString("X4").PadLeft(4, '0'));
                            startingLoc += 2;
                        }
                        else if (operation[i][1][0] == '+')
                        {
                            LOC.Add(startingLoc.ToString("X4").PadLeft(4, '0'));
                            startingLoc += 4;
                        }
                        else if (operation[i][1][operation[i][1].Length - 1] == 'R')
                        {
                            LOC.Add(startingLoc.ToString("X4").PadLeft(4, '0'));
                            startingLoc += 2;
                        }
                        else
                        {
                            LOC.Add(startingLoc.ToString("X4").PadLeft(4, '0'));
                            startingLoc += 3;
                        }
                    }
                    else if (operation[i].Count == 2)
                    {
                        if (operation[i][0] == "RSUB")
                        {
                            LOC.Add(startingLoc.ToString("X4").PadLeft(4, '0'));
                            startingLoc += 3;
                        }
                        else if (operation[i][0] == "."|| operation[i][0] == "BASE")
                        {
                            LOC.Add("");
                        }
                        else if (operation[i][0] == "CLEAR")
                        {
                            LOC.Add(startingLoc.ToString("X4").PadLeft(4, '0'));
                            startingLoc += 2;
                        }
                        else if (operation[i][0][0] == '+')
                        {
                            LOC.Add(startingLoc.ToString("X4").PadLeft(4, '0'));
                            startingLoc += 4;
                        }
                        else if (operation[i][0][operation[i][0].Length - 1] == 'R')
                        {
                            LOC.Add(startingLoc.ToString("X4").PadLeft(4, '0'));
                            startingLoc += 2;
                        }
                        else
                        {
                            LOC.Add(startingLoc.ToString("X4").PadLeft(4, '0'));
                            startingLoc += 3;
                        }
                    }
                    else
                    {
                        if (operation[i][0] == "RSUB")
                        {
                            LOC.Add(startingLoc.ToString("X4").PadLeft(4, '0'));
                            startingLoc += 3;
                        }
                        else
                        LOC.Add("");
                    }
                }
            }
            public void CalculateOBJCode()
            {
                for (int i = 0; i < operation.Count; i++)
                {
                    var xbpe = "0000";
                    var isType4 = false;
                    try
                    {


                        if (operation[i][0] == "BASE")
                        {
                            baseLoc = SymtabPair[operation[i][1]];
                        }
                        if (operation[i][0] == ".")
                        {
                            OBJCODE.Add("command");
                        }
                        else if (operation[i].Count == 3)
                        {
                            var command = operation[i][1];

                            if (command[0] == '+')
                            {
                                command = command.Remove(0, 1);
                                xbpe = "0001";
                                isType4 = true;
                            }
                            if (operation[i][1] == "EQU")
                            {
                                OBJCODE.Add("None");
                            }
                            else if (operation[i][1] == "CLEAR")
                            {
                                var lastpart = register[operation[i][2]] + '0';
                                OBJCODE.Add(CaternateStringF2("B4", lastpart));

                            }
                            else if (operation[i][2][0] == '#')
                            {
                                var output = OperationWithHashtag(command, operation[i][2].Remove(0, 1), xbpe, isType4, i);
                                OBJCODE.Add(output);
                            }
                            else if (operation[i][2][0] == '@')
                            {
                                var output = OperationWithAt(command, operation[i][2].Remove(0, 1), xbpe, isType4, i);
                                OBJCODE.Add(output);
                            }
                            else
                            {
                                var output = OperationNormal(command, operation[i][2], xbpe, isType4, i);
                                OBJCODE.Add(output);
                            }
                        }
                        else if (operation[i].Count == 2)
                        {
                            var command = operation[i][0];
                            if (command[0] == '+')
                            {
                                command = command.Remove(0, 1);
                                xbpe = "0001";
                                isType4 = true;
                            }
                            if (command == "CLEAR")
                            {
                                var lastpart = register[operation[i][1]] + '0';
                                OBJCODE.Add(CaternateStringF2("B4", lastpart));
                            }
                            else if (operation[i][0] == "TIXR")
                            {
                                var lastpart = register[operation[i][1]] + '0';
                                OBJCODE.Add(CaternateStringF2("B8", lastpart));
                            }
                            else if (operation[i][1][0] == '#')
                            {
                                var output = OperationWithHashtag(command, operation[i][1].Remove(0, 1), xbpe, isType4, i);
                                OBJCODE.Add(output);
                            }
                            else if (operation[i][1][0] == '@')
                            {
                                var output = OperationWithAt(command, operation[i][1].Remove(0, 1), xbpe, isType4, i);
                                OBJCODE.Add(output);
                            }
                            else
                            {
                                var output = OperationNormal(command, operation[i][1], xbpe, isType4, i);
                                OBJCODE.Add(output);
                            }
                        }
                        else
                        {
                            if (operation[i][0] != "END")
                                OBJCODE.Add(CaternateString("0x4F", "0", "000", "", ""));
                            else
                                OBJCODE.Add("");
                        }
                    }
                    catch(Exception e)
                    {
                        throw new Exception("your code have error");
                    }
                }
            }

            private string OperationNormal(string command, string info, string xbpe, bool isType4,int index)
            {
                var opni="None";
                var nixbpe = "";
                var lastpart = "";
                var format = "";
                var distance = 0;
                if (opcode.ContainsKey(command))
                {
                    opni = opcode[command];
                }
                else
                {
                    if(command=="BYTE")
                    {
                        if (info[0] == 'C')
                        {
                            info=info.Remove(info.Length - 1, 1);
                            info=info.Remove(0, 2);
                            var ret = "";
                            for (int i = 0; i < info.Length; i++)
                            {
                                ret += Convert.ToInt32(info[i]).ToString("X2");
                            }
                            return ret;
                        }
                        else if (info[0]=='X')
                        {
                            info=info.Remove(info.Length - 1, 1);
                            info=info.Remove(0, 2);
                            return info;
                        }
                    }
                    else
                    {
                        return "None";
                    }
                }
                opni = IncludeOffset(opni, 3);
                if(isType4)
                {
                    xbpe = "0001";
                    return CaternateString(opni,xbpe, SymtabPair[info.ToUpper()].ToString("X5"),"format4","11"+xbpe);
                }
                if (info.Contains(','))
                {
                    var item=info.Split(',');
                    if (command[command.Length-1]=='R')
                    {
                        format = "format2";
                        lastpart = register[item[0]] + register[item[1]];
                        opni = IncludeOffset(opni, -3);
                        return CaternateStringF2(opni, lastpart);
                    }
                    var temp = "";
                    for (int i = 0; i < xbpe.Length; i++)
                    {
                        if (i == 0) temp += '1';
                        else temp += xbpe[i];
                    }
                    xbpe = temp;
                    distance = SymtabPair[item[0]] -Convert.ToInt32(LOC[index],16);
                    if (isType4) distance -= 4;
                    else if (command[command.Length - 1] == 'R') distance -= 2;
                    else distance -= 3;
                    if (Math.Abs(distance) > 2047) 
                    {
                        var temp1 = "";
                        for (int i = 0; i < xbpe.Length; i++)
                        {
                            if (i == 1) temp1 += '1';
                            else temp1 += xbpe[i];
                        }
                        xbpe = temp1;
                        format = "base-relative";
                        distance = SymtabPair[item[0]] - baseLoc;
                        if(Math.Abs(distance)>4096)
                        {
                            throw new Exception($"command {command} please use type 4");
                        }
                    }
                    else
                    {
                        var temp1 = "";
                        for (int i = 0; i < xbpe.Length; i++)
                        {
                            if (i == 2) temp1 += '1';
                            else temp1 += xbpe[i];
                        }
                        xbpe = temp1;
                        format = "pc-relative";
                    }
                    nixbpe = "11" + xbpe;
                    lastpart = distance.ToString("X3");
                    return CaternateString(opni,xbpe, lastpart,format,nixbpe);
                }
                if(info.Contains('+'))
                {
                    var item=info.Split("+");
                    distance = SymtabPair[item[0]] - Convert.ToInt32(LOC[index], 16);
                    if (isType4) distance -= 4;
                    else if (command[command.Length - 1] == 'R') distance -= 2;
                    else distance -= 3;
                    distance += Convert.ToInt32(item[1]);
                    if (Math.Abs(distance) > 2047)
                    {
                        var temp1 = "";
                        for (int i = 0; i < xbpe.Length; i++)
                        {
                            if (i == 1) temp1 += '1';
                            else temp1 += xbpe[i];
                        }
                        xbpe = temp1;
                        format = "base-relative";
                        distance = SymtabPair[item[0]] + Convert.ToInt32(item[1] )- baseLoc;
                        if (Math.Abs(distance) > 4096)
                        {
                            throw new Exception($"command {command} please use type 4");
                        }
                    }
                    else
                    {
                        var temp1 = "";
                        for (int i = 0; i < xbpe.Length; i++)
                        {
                            if (i == 2) temp1 += '1';
                            else temp1 += xbpe[i];
                        }
                        xbpe = temp1;
                        format = "pc-relative";
                    }
                    nixbpe = "11" + xbpe;
                    lastpart = distance.ToString("X3");
                    return CaternateString(opni, xbpe, lastpart, format, nixbpe);


                }
                distance = SymtabPair[info.ToUpper()] - Convert.ToInt32(LOC[index],16);
                if (isType4) distance -= 4;
                else if (command[command.Length - 1] == 'R') distance -= 2;
                else distance -= 3;
                if (Math.Abs(distance) > 2047)
                {
                    var temp = "";
                    for (int i = 0; i < xbpe.Length; i++)
                    {
                        if (i == 1) temp += '1';
                        else temp += xbpe[i];
                    }
                    xbpe = temp;
                    format = "base-relative";
                    distance = SymtabPair[info.ToUpper()] - baseLoc;
                    if (Math.Abs(distance) > 4096)
                    {
                        throw new Exception($"command {command} please use type 4");
                    }
                }
                else
                {
                    var temp = "";
                    for (int i = 0; i < xbpe.Length; i++)
                    {
                        if (i == 2) temp += '1';
                        else temp += xbpe[i];
                    }
                    xbpe = temp;
                    format = "pc-relative";
                }
                nixbpe = "11" + xbpe;
                lastpart = distance.ToString("X3");
                return CaternateString(opni, xbpe, lastpart, format, nixbpe);
            }

            private string OperationWithAt(string command, string info, string xbpe, bool isType4,int index)
            {
                var opni = opcode[command];
                opni = IncludeOffset(opni, 2);
                var format = "";
                if (isType4)
                {
                    xbpe = "0001";
                    return CaternateString(opni, xbpe, SymtabPair[info.ToUpper()].ToString("X5"), "format4", "11" + xbpe);
                }
                var distance = SymtabPair[info] - Convert.ToInt32(LOC[index], 16) ;
                if (isType4) distance -= 4;
                else if (command[command.Length - 1] == 'R') distance -= 2;
                else distance -= 3;
                if (distance > 2047)
                {
                    var temp="";
                    for (int i = 0; i < xbpe.Length; i++)
                    {
                        if (i == 1) temp += '1';
                        else temp += xbpe[i];
                    }
                    xbpe=temp;
                    format = "base-relative";
                    distance = SymtabPair[info] - baseLoc;
                    if (Math.Abs(distance) > 4096)
                    {
                        throw new Exception($"command {command} please use type 4");
                    }
                }
                else
                {
                    var temp = "";
                    for (int i = 0; i < xbpe.Length; i++)
                    {
                        if (i == 2) temp += '1';
                        else temp += xbpe[i];
                    }
                    xbpe = temp;
                    format = "pc-relative";
                }
                var nixbpe = "10" + xbpe;
                var lastpart = distance.ToString("X3");
                return CaternateString(opni, xbpe, lastpart, format, nixbpe);
                
            }

            private string OperationWithHashtag(string command, string info, string xbpe, bool isType4,int index)
            {
                var opni = opcode[command];
                opni = IncludeOffset(opni, 1);
                var nixbpe = "";
                var immidate = "";
                var format = "";
                if (isType4)
                {
                    format = "format4";
                    nixbpe = "010001";
                    xbpe = "0001";
                    if (SymtabPair.ContainsKey(info))
                    {

                        immidate = (Convert.ToInt32(SymtabPair[info])).ToString("X5");
                    }
                    else
                    {
                        immidate = (Convert.ToInt32(info)).ToString("X5");
                    }
                }
                else
                {
                    format = "pc-relative";
                    nixbpe = "010010";
                    xbpe = "0010";
                    if (SymtabPair.ContainsKey(info))
                    {
                        var distance = SymtabPair[info] - Convert.ToInt32(LOC[index],16);
                        if (isType4) distance -= 4;
                        else if (command[command.Length - 1] == 'R') distance -= 2;
                        else distance -= 3;
                        immidate = distance.ToString("X3");
                    }
                    else
                    {
                        xbpe = "0000";
                        immidate = (Convert.ToInt32(info)).ToString("X3");
                    }
                }
                return CaternateString(opni,xbpe,immidate,format,nixbpe);
            }

            private string CaternateString(string opni, string xbpe, string lastpart,string format,string nixbpe)
            {
                if (lastpart.Length > 3 && xbpe[3]!='1')
                {
                    lastpart = lastpart.Substring(lastpart.Length - 3);
                }
                return $"{opni}{Convert.ToInt32(xbpe,2).ToString("X")}{lastpart.PadRight(6,' ')} , {format.PadRight(12, ' ')} , {nixbpe} ";
            }

            private string CaternateStringF2(string opni, string lastpart)
            {
                return $"{opni}{lastpart.PadRight(10,' ')}, format2";
            }
            
            public string IncludeOffset(string temp, int type)
            {
                char lastNum = temp[temp.Length-1];
                int ascii = hexTable[lastNum] + type;
                string newChar = ascii.ToString("X1");
                return temp.Substring(0, temp.Length - 1) + newChar;
            }
        }
    }
}