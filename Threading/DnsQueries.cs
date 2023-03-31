using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Threading
{
    internal class DnsQueries
    {
        private static readonly string[] dnsIpList = new string[]
        {
            "8.8.8.8",
            "208.67.222.222",
            "64.6.64.6",
            "9.9.9.9",
            "77.88.8.8"
        };

        public static void RunExample() 
        {
            var domain = "reg.ru";
            foreach (var dnsIp in dnsIpList)
            {
                SendDnsRequest(domain, dnsIp);
            }
            Console.ReadLine();
        }

        private static string SendDnsRequest(string domain, string dnsIp)
        {
            var sw = Stopwatch.StartNew();
            var dns = new DNSRequest();
            var endpoint = new IPEndPoint(IPAddress.Parse(dnsIp), 53);
            var ip = dns.SendAsync(domain, endpoint).GetAwaiter().GetResult();
            sw.Stop();
            var result = $"{domain} > {ip} ({dnsIp}, {sw.ElapsedMilliseconds} мс)";
            Console.WriteLine(result);
            return result;
        }
    }

    public class DNSRequest
    {
        private readonly byte[] HEADER = new byte[]
        {
            0xAA, 0xAA,
            0x01, 0x00,
            0x00, 0x01,
            0x00, 0x00,
            0x00, 0x00,
            0x00, 0x00
        };

        private readonly byte[] REQUEST_SUFFIX = new byte[]
        {
            0x00,
            0x00, 0x01,
            0x00, 0x01
        };

        private readonly bool debug;

        public DNSRequest(bool debug = false)
        {
            this.debug = debug;
        }

        public async Task<string> SendAsync(string domain, IPEndPoint dns)
        {
            var data = GetPackage(domain);

            var udpClient = new UdpClient();
            await udpClient.SendAsync(data, data.Length, dns);
            ShowBytes("Sended ", data);

            var receiveBytes = (await udpClient.ReceiveAsync()).Buffer;
            ShowBytes("Receiv ", receiveBytes);

            var rnd = new Random();
            System.Threading.Thread.Sleep(rnd.Next(2000));

            if (receiveBytes[0] == 0xAA && receiveBytes[1] == 0xAA && receiveBytes[2] == 0x81 && receiveBytes[3] == 0x80)
            {
                for (int i = 8; i < data.Length; i++)
                {
                    if (data[i] != receiveBytes[i])
                    {
                        throw new Exception("Получен неожиданный ответ");
                    }
                }

                var offset = data.Length + 12;
                var ip = string.Join(".", receiveBytes.Skip(offset).Select(x => x.ToString()).ToArray());
                if (debug)
                {
                    Console.WriteLine($"ansfer {ip}");
                }
                return ip;
            }
            else
            {
                throw new Exception("Ошибка в заголовке ответа");
            }
        }

        private void ShowBytes(string caption, IEnumerable<byte> data)
        {
            if (!debug) return;

            Console.Write(caption);
            foreach (var item in data)
            {
                Console.Write($"{item:X2} ");
            }
            Console.WriteLine();
        }

        private byte[] GetPackage(string domain)
        {
            var labels = domain.Split('.').SelectMany(ConvertLabels).ToArray();

            var data = HEADER.ToList();
            data.AddRange(labels);
            data.AddRange(REQUEST_SUFFIX);

            return data.ToArray();
        }

        private IEnumerable<byte> ConvertLabels(string label)
        {
            yield return (byte)label.Length;

            foreach (var item in Encoding.ASCII.GetBytes(label))
            {
                yield return item;
            }
        }
    }
}
