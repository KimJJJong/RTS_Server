using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using OfficeOpenXml;

class PacketExcelGenerator
{
    static string xmlPath = "../PDL.xml"; // 패킷 정의 XML
    static string excelPath = "PacketFlow.xlsx"; // 저장할 엑셀 파일

    public static void Init()
    {
        List<string> packetNames = ParsePDL(xmlPath);

        // 엑셀 업데이트 (첫 번째 열만)
        UpdateExcel(packetNames, excelPath);

        Console.WriteLine("엑셀 업데이트 완료: " + excelPath);

    }

    static List<string> ParsePDL(string xmlPath)
    {
        List<string> packetNames = new List<string>();
        XDocument doc = XDocument.Load(xmlPath);

        foreach (var packet in doc.Descendants("packet"))
        {
            string name = packet.Attribute("name")?.Value;
            if (!string.IsNullOrEmpty(name))
                packetNames.Add(name);
        }

        return packetNames;
    }

    static void UpdateExcel(List<string> packetNames, string excelPath)
    {
        FileInfo fileInfo = new FileInfo(excelPath);
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

        using (ExcelPackage package = fileInfo.Exists ? new ExcelPackage(fileInfo) : new ExcelPackage())
        {
            ExcelWorksheet worksheet = package.Workbook.Worksheets.FirstOrDefault() ?? package.Workbook.Worksheets.Add("Packet Flow");

            // 헤더가 없으면 자동 추가
            if (worksheet.Cells[1, 1].Value == null)
            {
                worksheet.Cells[1, 1].Value = "패킷 이름";
                worksheet.Cells[1, 2].Value = "호출 위치 (파일)";
                worksheet.Cells[1, 3].Value = "호출 함수";
                worksheet.Cells[1, 4].Value = "설명";
                worksheet.Cells[1, 5].Value = "데이터 필드";
            }

            int row = 2; // 1행은 헤더
            foreach (var packetName in packetNames)
            {
                worksheet.Cells[row, 1].Value = packetName;
                row++;
            }

            package.SaveAs(fileInfo);
        }

    }
}
