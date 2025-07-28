window.downloadCsvFromStream = (filename, base64Str) => {
    const link = document.createElement('a');
    link.download = filename;
    link.href = 'data:text/csv;base64,' + base64Str;
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
}