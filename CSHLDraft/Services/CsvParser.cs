﻿using System.Diagnostics.CodeAnalysis;
using Microsoft.JSInterop;
using Microsoft.AspNetCore.Components.Forms;
using CsvHelper;
using System.Globalization;
using CsvHelper.Configuration;


namespace CSHLDraft.Data;

public class CsvParser
{

    private readonly IJSRuntime _jsRuntime; // for downloading files
    
    private CsvConfiguration _csvConfiguration;

    public CsvParser(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
        _csvConfiguration = new(CultureInfo.InvariantCulture)
        {
            PrepareHeaderForMatch = args => args.Header.Trim().ToLower()
        };
    }


    public async Task<FileUploadResult<T>> ParseCsvAsync<T>(IBrowserFile file)
    {
        var retval = new FileUploadResult<T> {  FileName = file.Name };

        try
        {
            using var stream = new MemoryStream();
            await file.OpenReadStream().CopyToAsync(stream);
            stream.Seek(0, SeekOrigin.Begin);
            using var reader = new StreamReader(stream);

            using var csv = new CsvReader(reader, _csvConfiguration);

            var records = csv.GetRecords<T>() ?? throw new InvalidDataException();

            retval.Data = records.ToList();
        }
        catch (Exception e)
        {
            retval.Message = "There was an error when parsing the file...";
        }

        return retval;
    }
}


public class FileUploadResult<T>
{
    public string FileName { get; set; } = string.Empty;
    public IEnumerable<T>? Data { get; set; }
    public string Message { get; set; } = string.Empty;
    
    [MemberNotNullWhen( returnValue: true, nameof(Data))]
    public bool Success => Data is not null;
}