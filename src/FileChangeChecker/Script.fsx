#r "../../packages/Jil/lib/net45/Jil.dll"
#r"../../packages/Sigil/lib/net45/Sigil.dll"

open System
open System.IO
open System.Collections.Generic
open System.Security.Cryptography
open Microsoft.FSharp.Core
open Jil

type HashAlgorithm =
    | MD5
    | SHA1
    | SHA256
    | SHA384
    | SHA512

let calculateFileHash (hashAlgorithm:HashAlgorithm) filePath  =
    use hasher = HashAlgorithm.Create(hashAlgorithm.ToString())
    filePath 
    |> File.ReadAllBytes 
    |> hasher.ComputeHash
    |> BitConverter.ToString
    |> (fun hash -> hash.Replace("-", String.Empty))

let calculateFileSHA256=
    calculateFileHash HashAlgorithm.SHA256

let checkFileHash hashAlgorithm originalHash filePath =
    let hash = calculateFileHash hashAlgorithm filePath
    hash = originalHash

let checkFileSHA256 =
    checkFileHash HashAlgorithm.SHA256


let hashDirectoryFiles hashAlgorithm dir resultPath =
    Directory.GetFiles(dir, "*", SearchOption.AllDirectories) 
    |> Array.Parallel.map (fun filePath -> filePath, (calculateFileHash hashAlgorithm filePath))
    |> dict
    |> (fun map -> JSON.Serialize(map, Options.PrettyPrint))
    |> (fun json -> File.WriteAllText(resultPath, json))

let hashDirectoryFilesSHA256 =
    hashDirectoryFiles HashAlgorithm.SHA256

let checkDictionaryFiles hashAlgorithm hashesFilePath dir resultPath=
    let json = File.ReadAllText hashesFilePath
    let hashes:Dictionary<string,string> = JSON.Deserialize(json)
    Directory.GetFiles(dir, "*", SearchOption.AllDirectories) 
    |> Array.Parallel.map (fun filePath -> (checkFileHash hashAlgorithm (hashes.Item(filePath)) filePath).ToString(), filePath)
    // |> dict
    |> (fun map -> JSON.Serialize(map, Options.PrettyPrint))
    |> (fun json -> File.WriteAllText(resultPath, json))

let checkDictionaryFilesSHA256 =
    checkDictionaryFiles HashAlgorithm.SHA256


    
// Playground
#time "on"
let dir = "c:/Development"
let resultPath = Path.Combine(dir, "hashes.txt")
hashDirectoryFilesSHA256 dir resultPath
let checkedResultPath = Path.Combine(dir, "checks.txt")
checkDictionaryFilesSHA256 resultPath dir checkedResultPath

#time "off"

