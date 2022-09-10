using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Threading;
using UnityEngine;

public class SimThread : MonoBehaviour
{
    public bool initThread = false;
    private Thread _simThread = null;
 
    static string _worldPath = null;
    
    // Start is called before the first frame update
    void Start()
    {
        _worldPath = Path.Join(Application.persistentDataPath, "level.dat");
        if (!initThread) return;
        _simThread = new Thread(Simulate);
        _simThread.Start();
    }

    private static void Simulate()
    {
        if (!File.Exists(_worldPath))
        {
            File.Create(_worldPath).Close();
        }
        
        MemoryMappedFile level = MemoryMappedFile.CreateFromFile(_worldPath,FileMode.Open,"mem_level",6_000_000_000,MemoryMappedFileAccess.ReadWrite);
        long currOffset = 0;
        
    }
}
