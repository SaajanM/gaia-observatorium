using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Threading;
using UnityEngine;

public class SimController : MonoBehaviour
{
    public bool initThread = false;
    private Thread _simThread = null;
 
    static string _worldPath = null;

    public static MemoryMappedViewAccessor accessor = null;
    // Start is called before the first frame update
    void Start()
    {
        _worldPath = Path.GetFullPath("~/Documents/games/gaia/worlds/test1.gaia");
        if (!initThread) return;
        StartController();
    }

    private static void StartController()
    {
        if (!File.Exists(_worldPath))
        {
            throw new FileNotFoundException();
        }
        
        MemoryMappedFile world = MemoryMappedFile.CreateFromFile(_worldPath,FileMode.Open,"mem_level",10L*1024L*1024L*1024L,MemoryMappedFileAccess.Read);
        accessor = world.CreateViewAccessor();


    }
    
}
