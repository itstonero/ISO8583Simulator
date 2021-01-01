using OpenIso8583Net;
using OpenIso8583Net.FieldValidator;
using OpenIso8583Net.Formatter;
using System;
using System.Collections.Generic;
using System.Text;

namespace Simulator
{
    public class ISOSerializer
    {
        public static string ClearZMK { get; set; }
        public static byte[] Compose(Transactions transType, string transTime)
        {
            var isoMsg = new Iso8583Extended
            {
                MessageType = Iso8583.MsgType._0800_NWRK_MNG_REQ
            };

            isoMsg[Iso8583.Bit._011_SYS_TRACE_AUDIT_NUM] = transTime.Substring(0, 6);
            isoMsg[Iso8583.Bit._012_LOCAL_TRAN_TIME] = transTime.Substring(4);
            isoMsg[Iso8583.Bit._013_LOCAL_TRAN_DATE] = transTime.Substring(0, 4);
            isoMsg[Iso8583.Bit._007_TRAN_DATE_TIME] = transTime;
            isoMsg[Iso8583.Bit._041_CARD_ACCEPTOR_TERMINAL_ID] = "201116FH";
            string processingCode = string.Empty;
            switch (transType)
            {
                case Transactions.MasterKey:
                    processingCode = "9A0000";
                    break;
                case Transactions.SessionKey:
                    processingCode = "9B0000";
                    break;
                case Transactions.PinKey:
                    processingCode = "9G0000";
                    break;
                case Transactions.Parameter_Download:
                    processingCode = "9C0000";
                    isoMsg[Iso8583Rev93.Bit._062_HOTCARD_CAPACITY] = "010083K226873";
                    isoMsg[Iso8583Rev93.Bit._064_MAC] = "5636DAB244449CD855CAA894C9C488F0C8855784C41F0D2CEB2ED1C5BAC89B84";
                    /*
                        [064] = > 5636DAB244449CD855CAA894C9C488F0C8855784C41F0D2CEB2ED1C5BAC89B84
                     */
                    break;
                case Transactions.End_Of_Day:
                    processingCode = "9H0000";
                    break;
                case Transactions.Call_Home:
                    processingCode = "9D0000";
                    isoMsg[Iso8583Rev93.Bit._062_HOTCARD_CAPACITY] = "010083K226873090083K226873100032.1";
                    /*
                       [064] = > 99514FA43D3080399D081BC041986E10F03B3D8F9748C1BBA075DEDDA3606B12
                     */
                    break;
                case Transactions.Fnx_Tnx:
                    processingCode = "000000";
                    /*
                    F:Logger.c|L:00041| [000] = > 0200

                    F:Logger.c|L:00041| [002] = > 5061230122647035089

                    F:Logger.c|L:00041| [003] = > 000000

                    F:Logger.c|L:00041| [004] = > 000000000200

                    F:Logger.c|L:00041| [007] = > 1106153049

                    F:Logger.c|L:00041| [011] = > 002792

                    F:Logger.c|L:00041| [012] = > 153049

                    F:Logger.c|L:00041| [013] = > 1106

                    F:Logger.c|L:00041| [014] = > 2012

                    F:Logger.c|L:00041| [018] = > 5999

                    F:Logger.c|L:00041| [022] = > 051

                    F:Logger.c|L:00041| [023] = > 000

                    F:Logger.c|L:00041| [025] = > 00

                    F:Logger.c|L:00041| [026] = > 12

                    F:Logger.c|L:00041| [028] = > D00000000

                    F:Logger.c|L:00041| [032] = > 506123

                    F:Logger.c|L:00041| [035] = > 5061230122647035089D2012601002857591

                    F:Logger.c|L:00041| [037] = > 872834768553

                    F:Logger.c|L:00041| [040] = > 601

                    F:Logger.c|L:00041| [041] = > 2035F273

                    F:Logger.c|L:00041| [042] = > 2011LA024839532

                    F:Logger.c|L:00041| [043] = > POS COLLECTIONS ACCO    LA          LANG

                    F:Logger.c|L:00041| [049] = > 566

                    F:Logger.c|L:00041| [055] = > 820258008407A0000003710001950502800080009A032011069C01005F2A0205665F3401009F02060000000002009F03060000000000009F0607A00000037100019F090200029F10200FA501A239F8000000000000000000000F0100000000000000000000000000009F1A0205669F1E08334B3232363837339F26082467965B2C7E08BF9F2701809F3303E0F8C89F34034103029F3501229F3602013A9F3704C882A0E99F4104000027928E0E0000C35000000000410342031F069F530152

                    F:Logger.c|L:00041| [123] = > 51011151134C101

                    F:Logger.c|L:00041| [128] = > 49D0688BA785842A896FB9E0F2D2C3BA142D42AB34F166B403D54B4880709445
                     */
                    break;
            }
            isoMsg[Iso8583.Bit._003_PROC_CODE] = processingCode;
            //isoMsg[Iso8583Rev93.Bit._062_HOTCARD_CAPACITY] = serialNumber;
            Console.WriteLine(isoMsg.ToString());
            var isoMessage = new List<byte> { (byte)(isoMsg.PackedLength / 256), (byte)(isoMsg.PackedLength % 256) };
            isoMessage.AddRange(isoMsg.ToMsg());
            return isoMessage.ToArray();
        }


    }
    public class Iso8583Extended : Iso8583
    {
        private static readonly Template ExtendedTemplate;
        static Iso8583Extended()
        {
            ExtendedTemplate = GetDefaultIso8583Template();
            ExtendedTemplate.BitmapFormatter = Formatters.Ascii;
            ExtendedTemplate[Iso8583.Bit._003_PROC_CODE] = FieldDescriptor.AsciiAlphaNumeric(6);
            ExtendedTemplate[Iso8583.Bit._053_SECURITY_RELATED_CONTROL_INFORMATION] = FieldDescriptor.AsciiAlphaNumeric(38);
            ExtendedTemplate[Iso8583Rev93.Bit._123_RECEIPT_DATA] = FieldDescriptor.AsciiLllCharacter(999);
            ExtendedTemplate[Iso8583Rev93.Bit._062_HOTCARD_CAPACITY] = FieldDescriptor.AsciiLllCharacter(999);
            ExtendedTemplate[Iso8583Rev93.Bit._064_MAC] = FieldDescriptor.AsciiFixed(64, FieldValidators.Hex);
            ExtendedTemplate[Iso8583Rev93.Bit._128_MAC] = FieldDescriptor.AsciiFixed(64, FieldValidators.Hex);
            ExtendedTemplate[Iso8583Rev93.Bit._055_ICC_DATA] = FieldDescriptor.AsciiLllCharacter(999);
            ExtendedTemplate[Iso8583Rev93.Bit._038_APPROVAL_CODE] = FieldDescriptor.AsciiFixed(6, FieldValidators.Anp);
        }


        public Iso8583Extended() : base(ExtendedTemplate)
        {

        }
    }

    public class ResponseData
    {
        public string ResponseCode { get; set; }
        public string ResponseMessage { get; set; }
    }

    public class SimData
    {
        public string SerialNo { get; set; }
        public string APN { get; set; }
        public string Network { get; set; }
    }

    public class HostData
    {
        public string CombinedKey { get; set; }
        public string Kcv { get; set; }
    }
    public class VasData
    {
        public string Title { get; set; }
        public string VasTerminalId { get; set; }
        public string MasterKey { get; set; }
        public string SessionKey { get; set; }
        public string PinKey { get; set; }
    }

    public class KeyData
    {
        public KeyData()
        {
            MasterHex = new byte[16];
            SessionHex = new byte[16];
            PinHex = new byte[16];
            MasterKey = String.Empty;
            SessionKey = String.Empty;
            PinKey = String.Empty;
        }

        public string MasterKey { get; set; }
        public string SessionKey { get; set; }
        public string PinKey { get; set; }

        public byte[] MasterHex { get; set; }
        public byte[] SessionHex { get; set; }
        public byte[] PinHex { get; set; }
        public override string ToString()
        {
            return $"############################################\n\n\n[ 1. ] MASTER KEY >> {MasterKey}\n[ 2. ] SESSION KEY >> {SessionKey}\n[ 3. ] PIN KEY {PinKey}\n\n\n############################################";
        }

        public void ConvertKeysToHex()
        {

            if (MasterKey.Length >= 32)
                ParseKeys(Transactions.MasterKey);
            if (SessionKey.Length >= 32)
                ParseKeys(Transactions.SessionKey);
            if (PinKey.Length >= 32)
                ParseKeys(Transactions.PinKey);
            ToString();
        }

        private void ParseKeys(Transactions keyType)
        {
            switch(keyType)
            {
                case Transactions.MasterKey:
                    for (int i = 0, j = 0; i < 32;)
                    {
                        MasterHex[j] &= (byte)(MasterKey[i] << 4);
                        MasterHex[j] &= (byte)(MasterKey[i + 1]);
                        i += 2;
                        j += 1;
                    }
                    Console.WriteLine($"MASTER HEX ( {MasterHex.Length} ) :: { Encoding.ASCII.GetString(MasterHex) }");
                    break;
                case Transactions.SessionKey:
                    for (int i = 0, j = 0; i < 32;)
                    {
                        SessionHex[j] &= (byte)(SessionKey[i] << 4);
                        SessionHex[j] &= (byte)(SessionKey[i + 1]);
                        i += 2;
                        j += 1;
                    }
                    Console.WriteLine($"SESSION HEX ( {SessionHex.Length} ) :: { Encoding.ASCII.GetString(SessionHex) }");
                    break;
                case Transactions.PinKey:
                    for (int i = 0, j = 0; i < 32;)
                    {
                        PinHex[j] &= (byte)(PinKey[i] << 4);
                        PinHex[j] &= (byte)(PinKey[i + 1]);
                        i += 2;
                        j += 1;
                    }
                    Console.WriteLine($"PIN HEX ( {PinHex.Length} ) :: { Encoding.ASCII.GetString(PinHex) }");
                    break;
            }
        }
    }
    public class TMSConfig
    {
        public ResponseData response { get; set; }
        public SimData SimConfig { get; set; }
        public List<int> Menu { get; set; }
        public string terminalID { get; set; }
        public string terminalSerial { get; set; }
        public string agentID { get; set; }
        public string agentName { get; set; }
        public string MerchantID { get; set; }
        public string MerchantName { get; set; }
        public string IpAddress { get; set; }
        public string Port { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Location { get; set; }
        public string TerminalMode { get; set; }
        public string InstitutionCode { get; set; }
        public string ConsultantCode { get; set; }
        public string TestPlatform { get; set; }
        public string receiptHeader { get; set; }
        public string receiptFooter { get; set; }
        public string logo { get; set; }
        public HostData HostKeys { get; set; }
        public VasData VasConfig { get; set; }
        public string EnablePayAttitude { get; set; }
        public string PayAttitudeConfig { get; set; }
    }
}

/*
 * CONFIGURATION
 2020-11-09 09:30:54.018 12816-13357/com.africanvogue.avg I/System.out: Header bytes: 003c
2020-11-09 09:30:54.026 12816-13357/com.africanvogue.avg I/System.out: Request: ��<080022380000008000009A00001109093054093054093054110920390015
2020-11-09 09:30:54.026 12816-13357/com.africanvogue.avg I/System.out:  Hex: 003c303830303232333830303030303038303030303039413030303031313039303933303534303933303534303933303534313130393230333930303135
2020-11-09 09:30:54.805 12816-13357/com.africanvogue.avg W/System.err: SLF4J: Failed to load class "org.slf4j.impl.StaticLoggerBinder".
2020-11-09 09:30:54.806 12816-13357/com.africanvogue.avg W/System.err: SLF4J: Defaulting to no-operation (NOP) logger implementation
2020-11-09 09:30:54.806 12816-13357/com.africanvogue.avg W/System.err: SLF4J: See http://www.slf4j.org/codes.html#StaticLoggerBinder for further details.
2020-11-09 09:30:55.037 12816-13357/com.africanvogue.avg I/System.out: Header bytes: 003c
2020-11-09 09:30:55.038 12816-13357/com.africanvogue.avg I/System.out: Request: ��<080022380000008000009B00001109093055093055093055110920390015
2020-11-09 09:30:55.038 12816-13357/com.africanvogue.avg I/System.out:  Hex: 003c303830303232333830303030303038303030303039423030303031313039303933303535303933303535303933303535313130393230333930303135
2020-11-09 09:30:55.871 12816-13357/com.africanvogue.avg I/System.out: Header bytes: 003c
2020-11-09 09:30:55.872 12816-13357/com.africanvogue.avg I/System.out: Request: ��<080022380000008000009G00001109093055093055093055110920390015
2020-11-09 09:30:55.872 12816-13357/com.africanvogue.avg I/System.out:  Hex: 003c303830303232333830303030303038303030303039473030303031313039303933303535303933303535303933303535313130393230333930303135
2020-11-09 09:30:56.732 12816-13357/com.africanvogue.avg I/System.out: Header bytes: 008e
2020-11-09 09:30:56.733 12816-13357/com.africanvogue.avg I/System.out: Request: ���080022380000008000059C0000110909305609305609305611092039001501501010911200075651D199FC5BDB7FC5F1FE4AF0A8F7A904B26E049A488727C3DD8C559FF03D0E8D
2020-11-09 09:30:56.734 12816-13357/com.africanvogue.avg I/System.out:  Hex: 008e30383030323233383030303030303830303030353943303030303131303930393330353630393330353630393330353631313039323033393030313530313530313031303931313230303037353635314431393946433542444237464335463146453441463041384637413930344232364530343941343838373237433344443843353539464630334430453844
2020-11-09 09:31:09.873 12816-12816/com.africanvogue.avg W/ActivityThread: handleWindowVisibility: no activity for token android.os.BinderProxy@5777a94

PURCHASE
 */