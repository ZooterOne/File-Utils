using System;
using System.IO;
using System.Linq;
using System.Threading;

namespace DuplicateFinder.Tests;

public class FileCacheTests
{
    private FileCache? m_fileCache;

    [SetUp]
    public void Setup()
    {
        m_fileCache = new FileCache();
    }

    [Test]
    public void TestAddInvalidDirectory()
    {
        var directory = new DirectoryInfo("Not a directory");
        Assert.Throws<ArgumentException>(() => m_fileCache!.AddDirectory(directory, CancellationToken.None));
    }

    [Test]
    public void TestAddDirectory()
    {
        var directory = new DirectoryInfo("assets");
        Assert.DoesNotThrow(() => m_fileCache!.AddDirectory(directory, CancellationToken.None));
        Assert.That(m_fileCache!.Count(), Is.EqualTo(7));
    }

    [Test]
    public void TestAddInvalidFile()
    {
        var file = new FileInfo("Not a file");
        Assert.Throws<ArgumentException>(() => m_fileCache!.AddFile(file));
    }

    [Test]
    public void TestAddFile()
    {
        var file = new FileInfo("./assets/one.png");
        Assert.DoesNotThrow(() => m_fileCache!.AddFile(file));
        Assert.That(m_fileCache!.Count(), Is.EqualTo(1));
        file = new FileInfo("./assets/duplicates/three.png");
        Assert.DoesNotThrow(() => m_fileCache!.AddFile(file));
        Assert.That(m_fileCache!.Count(), Is.EqualTo(2));
    }

    [Test]
    public void TestFindAllDuplicates()
    {
        var directory = new DirectoryInfo("assets");
        Assert.DoesNotThrow(() => m_fileCache!.AddDirectory(directory, CancellationToken.None));
        var duplicates = m_fileCache!.FindAllDuplicates();
        Assert.That(duplicates, Has.Count.EqualTo(2));
        Assert.That(duplicates.Any(item => item.Contains(Path.Combine(directory.FullName, "one.png"))), Is.True);
        Assert.That(duplicates.Any(item => item.Contains(Path.Combine(directory.FullName, "duplicates/three.png"))), Is.True);
        Assert.That(duplicates.Any(item => item.Contains(Path.Combine(directory.FullName, "one.txt"))), Is.True);
        Assert.That(duplicates.Any(item => item.Contains(Path.Combine(directory.FullName, "duplicates/two.txt"))), Is.True);
        Assert.That(duplicates.Any(item => item.Contains(Path.Combine(directory.FullName, "four.txt"))), Is.True);
    }

    [Test]
    public void TestFindDuplicates()
    {
        var directory = new DirectoryInfo("assets/duplicates");
        Assert.DoesNotThrow(() => m_fileCache!.AddDirectory(directory, CancellationToken.None));
        var file = new FileInfo("./assets/one.png");
        Assert.That(m_fileCache!.FindDuplicates(file, out var matches), Is.True);
        Assert.That(matches, Has.Count.EqualTo(1));
        file = new FileInfo("./assets/three.txt");
        Assert.That(m_fileCache.FindDuplicates(file, out matches), Is.False);
        Assert.That(matches, Is.Empty);
        file = new FileInfo("./assets/one.txt");
        Assert.That(m_fileCache.FindDuplicates(file, out matches), Is.True);
        Assert.That(matches, Has.Count.EqualTo(1));
    }
    
    [Test]
    public void TestFindAllDistincts()
    {
        var directory = new DirectoryInfo("assets");
        Assert.DoesNotThrow(() => m_fileCache!.AddDirectory(directory, CancellationToken.None));
        var distincts = m_fileCache!.FindAllDistincts();
        Assert.That(distincts, Has.Count.EqualTo(2));
        Assert.That(distincts.Any(item => item.Contains(Path.Combine(directory.FullName, "two.png"))), Is.True);
        Assert.That(distincts.Any(item => item.Contains(Path.Combine(directory.FullName, "three.txt"))), Is.True);
    }

    [Test]
    public void TestFullProcess()
    {
        var directory = new DirectoryInfo("assets/duplicates");
        Assert.DoesNotThrow(() => m_fileCache!.AddDirectory(directory, CancellationToken.None));
        Assert.That(m_fileCache!.Count(), Is.EqualTo(2));
        directory = new DirectoryInfo("assets");
        var file = new FileInfo("./assets/one.png");
        Assert.DoesNotThrow(() => m_fileCache.AddFile(file));
        Assert.That(m_fileCache!.Count(), Is.EqualTo(3));
        var duplicates = m_fileCache!.FindAllDuplicates();
        Assert.That(duplicates, Has.Count.EqualTo(1));
        Assert.That(duplicates.Any(item => item.Contains(Path.Combine(directory.FullName, "one.png"))), Is.True);
    }

    [Test]
    public void TestSave()
    {
        var directory = new DirectoryInfo("assets");
        Assert.DoesNotThrow(() => m_fileCache!.AddDirectory(directory, CancellationToken.None));
        var filename = Path.Combine(directory.Parent!.FullName, "assets.cache");
        Assert.DoesNotThrow(() => m_fileCache!.Save(filename));
        Assert.That(File.Exists(filename), Is.True);
        File.Delete(filename);
    }
    
    [Test]
    public void TestOpenException()
    {
        var directory = new DirectoryInfo("assets");
        var filename = Path.Combine(directory.Parent!.FullName, "assets.cache");
        Assert.Throws<FileNotFoundException>(() => m_fileCache!.Load(filename, false, false));
    }
    
    [Test]
    public void TestOpen()
    {
        var directory = new DirectoryInfo("assets");
        var filename = Path.Combine(directory.Parent!.FullName, "assets.cache");
        var tempCache = new FileCache();
        Assert.DoesNotThrow(() => tempCache.AddDirectory(directory, CancellationToken.None));
        Assert.DoesNotThrow(() => tempCache.Save(filename));
        Assert.DoesNotThrow(() => m_fileCache!.Load(filename, false, false));
        Assert.That(m_fileCache!.Count(), Is.EqualTo(7));
        File.Delete(filename);
    }
    
    [Test]
    public void TestOpenWithMissingFile()
    {
        var directory = new DirectoryInfo("assets");
        var filename = Path.Combine(directory.Parent!.FullName, "assets.cache");
        var tempFile = Path.Combine(directory.FullName, "one_copy.png");
        File.Copy(Path.Combine(directory.FullName, "one.png"), tempFile, true);
        var tempCache = new FileCache();
        Assert.DoesNotThrow(() => tempCache.AddDirectory(directory, CancellationToken.None));
        Assert.DoesNotThrow(() => tempCache.Save(filename));
        File.Delete(tempFile);
        Assert.DoesNotThrow(() => m_fileCache!.Load(filename, false, false));
        Assert.That(m_fileCache!.Count(), Is.EqualTo(8));
        Assert.DoesNotThrow(() => m_fileCache!.Load(filename, true, false));
        Assert.That(m_fileCache!.Count(), Is.EqualTo(7));
        File.Delete(filename);
    }
    
    [Test]
    public void TestOpenWithAppend()
    {
        var directory = new DirectoryInfo("assets/duplicates");
        var filename = Path.Combine(directory.Parent!.Parent!.FullName, "assets.cache");
        var tempCache = new FileCache();
        Assert.DoesNotThrow(() => tempCache.AddDirectory(directory, CancellationToken.None));
        Assert.DoesNotThrow(() => tempCache.Save(filename));
        m_fileCache!.AddFile(new FileInfo("assets/three.txt"));
        Assert.DoesNotThrow(() => m_fileCache!.Load(filename, false, true));
        Assert.That(m_fileCache!.Count(), Is.EqualTo(3));
        Assert.DoesNotThrow(() => m_fileCache!.Load(filename, false, false));
        Assert.That(m_fileCache!.Count(), Is.EqualTo(2));
        File.Delete(filename);
    }
}