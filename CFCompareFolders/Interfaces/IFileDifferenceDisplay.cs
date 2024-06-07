namespace CFCompareFolders.Interfaces
{
    /// <summary>
    /// Interface for displaying differences between two files
    /// </summary>
    public interface IFileDifferenceDisplay
    {        
        void Display(string file1, string file2);
    }
}
