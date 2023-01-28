open System
open System.Collections.Generic
open System.IO
open System.Text
open Argu


let supportedExtensions = [|
    ".jpg"
    ".jpeg"
    ".png"
    ".gif"
    ".webp"
|]

type Arguments =
    | [<AltCommandLine("-d")>] Debug
    | [<AltCommandLine("-k")>] KeepOriginals
    | [<MainCommand; Last>] Paths of paths: string

    interface IArgParserTemplate with
        member s.Usage =
            match s with
            | Debug -> "run in debug mode (no files will be changed)"
            | KeepOriginals -> "keep original files in place instead of removing them"
            | Paths _ -> "paths to sort files in"

let parser = ArgumentParser.Create<Arguments>(programName = "Convert-Wd14Tags", helpTextMessage = "fix tag files output by wd14 tagger for use in hydrus network")

let argv = Array.skip 1 (Environment.GetCommandLineArgs())

let rec walkPaths(paths: seq<string>) =
    seq {
        for path in paths do
                let pathAttributes = File.GetAttributes path
                
                if pathAttributes.HasFlag FileAttributes.Directory then
                    yield! walkPaths (Directory.GetFiles(path))
                else
                    yield path
    }

let getWdTagFilePath (path: string) =
    let dir = Path.GetDirectoryName path
    let extensionless = Path.GetFileNameWithoutExtension path
    
    Path.Combine(dir, $"{extensionless}.txt")

let getHydrusTagFilePath (path: string) = $"{path}.txt"
            
let filterInvalidFiles (path: string) =
    let extension = Path.GetExtension path
    let dir = Path.GetDirectoryName path
    let extensionless = Path.GetFileNameWithoutExtension path
    
    let wdTagsPath = Path.Combine(dir, $"{extensionless}.txt")
    let hydrusTagsPath = $"{path}.txt"
    
    Array.contains extension supportedExtensions && File.Exists wdTagsPath && not <| File.Exists hydrusTagsPath

try
    let parsedArgs = parser.ParseCommandLine argv
    
    let debug = parsedArgs.Contains Debug
    
    let keepOriginals = parsedArgs.Contains KeepOriginals
    
    let paths = parsedArgs.GetResults Paths
    
    let rec convertWdTagFileToHydrus path =
        use reader = File.OpenText(getWdTagFilePath path)
        let contents = reader.ReadToEnd()
        
        let contents = contents.Replace(", ", "\n")
        
        let hydrusTagPath = getHydrusTagFilePath path
        
        Console.WriteLine $"copying tags from \"{Path.GetFileName path}\" to \"{Path.GetFileName hydrusTagPath}\""
        
        if not <| debug then
            use writer = File.OpenWrite hydrusTagPath
            
            Encoding.UTF8.GetBytes contents |> writer.Write
            
        if not <| keepOriginals then
            Console.WriteLine $"deleting \"{Path.GetFileName path}\""
            if not <| debug then
                File.Delete path
    
    paths |> walkPaths |> Seq.filter filterInvalidFiles |> Seq.iter convertWdTagFileToHydrus
with
    | :? ArguParseException -> Console.Out.WriteLine(parser.PrintUsage())
    | ex -> Console.Error.WriteLine(ex.Message)