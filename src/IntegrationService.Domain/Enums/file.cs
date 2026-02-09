class file
{
    public string Name { get; set; }
    public long Size { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime ModifiedDate { get; set; }

    public file (string name, long size, DateTime createdDate, DateTime modifiedDate)
    {
        Name = name;
        Size = size;
        CreatedDate = createdDate;
        ModifiedDate = modifiedDate;
    }

    public void DisplayInfo()
    {
        Console.WriteLine($"Name: {Name}");
        Console.WriteLine($"Size: {Size} bytes");
        Console.WriteLine($"Created Date: {CreatedDate}");
        Console.WriteLine($"Modified Date: {ModifiedDate}");

        Console.WriteLine($"Modified Date3: {ModifiedDate}");
        Console.WriteLine($"Modified Date4: {ModifiedDate}");


    }
}