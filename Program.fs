open System
open System.Collections.Generic

module Base =
    let inline existsInWin (mChar: char) (str: string) offset rad =
        let startAt = max 0 (offset - rad)
        let endAt = min (offset + rad) (String.length str - 1)  
        if endAt - startAt < 0 then false
        else
            let rec exists index =
                if str.[index] = mChar then true
                elif index = endAt then false
                else exists (index + 1)
            exists startAt
    
    let jaro s1 s2 =    
        // The radius is half of the lesser 
        // of the two string lengths rounded up.
        let matchRadius = 
            let minLen = 
                min (String.length s1) (String.length s2) in
                  minLen / 2 + minLen % 2
    
        // An inner function which recursively finds the number  
        // of matched characters within the radius.
        let commonChars (chars1: string) (chars2: string) =
            let rec inner i result = 
                match i with
                | -1 -> result
                | _ -> if existsInWin chars1.[i] chars2 i matchRadius
                       then inner (i - 1) (chars1.[i] :: result)
                       else inner (i - 1) result
            inner (chars1.Length - 1) []
    
        // The sets of common characters and their lengths as floats 
        let c1 = commonChars s1 s2
        let c2 = commonChars s2 s1
        let c1length = float (List.length c1)
        let c2length = float (List.length c2)
        
        // The number of transpositions within 
        // the sets of common characters.
        let transpositions = 
            let rec inner cl1 cl2 result = 
                match cl1, cl2 with
                | [], _ | _, [] -> result
                | c1h :: c1t, c2h :: c2t -> 
                    if c1h <> c2h
                    then inner c1t c2t (result + 1.0)
                    else inner c1t c2t result
            let mismatches = inner c1 c2 0.0
            // If one common string is longer than the other
            // each additional char counts as half a transposition
            (mismatches + abs (c1length - c2length)) / 2.0
    
        let s1length = float (String.length s1)
        let s2length = float (String.length s2)
        let tLength = max c1length c2length
    
        // The jaro distance as given by 
        // 1/3 ( m2/|s1| + m1/|s2| + (mc-t)/mc )
        let result = (c1length / s1length +
                      c2length / s2length + 
                      (tLength - transpositions) / tLength)
                     / 3.0
    
        // This is for cases where |s1|, |s2| or m are zero 
        if System.Double.IsNaN result then 0.0 else result

module Opt =
    open System.Buffers

    let inline existsInWin (mChar: char) (str: string) (offset: int) (rad: int) =
        let startAt = Math.Max(0, offset - rad)
        let endAt = Math.Min(offset + rad, str.Length - 1)  
        if endAt - startAt < 0 then false
        else
            let rec exists index =
                if str.[index] = mChar then true
                elif index = endAt then false
                else exists (index + 1)
            exists startAt
    
    let jaro (s1: string, s2: string, usePool: bool) =    
        // The radius is half of the lesser of the two string lengths rounded up.
        let matchRadius = 
            let minLen = Math.Min(s1.Length, s2.Length)
            minLen / 2 + minLen % 2
    
        // An inner function which recursively finds the number  
        // of matched characters within the radius.
        let commonChars (chars1: string) (chars2: string) (buff: char[]) : ArraySegment<char> =
            let mutable count = 0
            for i = 0 to chars1.Length - 1 do
                let c = chars1.[i]
                if existsInWin c chars2 i matchRadius then
                    buff.[count] <- c
                    count <- count + 1 
            ArraySegment(buff, 0, count)
    
        // The sets of common characters and their lengths as floats
        let buff1 = if usePool then ArrayPool.Shared.Rent s1.Length else Array.zeroCreate s1.Length 
        let buff2 = if usePool then ArrayPool.Shared.Rent s2.Length else Array.zeroCreate s2.Length 
        try 
            let c1 = commonChars s1 s2 buff1
            let c2 = commonChars s2 s1 buff2
            let c1length = float c1.Count
            let c2length = float c2.Count
            
            // The number of transpositions within the sets of common characters.
            let transpositions =
                let mutable mismatches = 0.0
                for i = 0 to (Math.Min(c1.Count, c2.Count)) - 1 do
                    if c1.[i] <> c2.[i] then 
                        mismatches <- mismatches + 1.0
                            
                // If one common string is longer than the other
                // each additional char counts as half a transposition
                (mismatches + abs (c1length - c2length)) / 2.0
        
            let tLength = Math.Max(c1length, c2length)
        
            // The jaro distance as given by 1/3 ( m2/|s1| + m1/|s2| + (mc-t)/mc )
            let result = (c1length / float s1.Length + c2length / float s2.Length + (tLength - transpositions) / tLength) / 3.0
        
            // This is for cases where |s1|, |s2| or m are zero 
            if Double.IsNaN result then 0.0 else result
        finally
            if usePool then
                ArrayPool.Shared.Return buff1
                ArrayPool.Shared.Return buff2
    
open BenchmarkDotNet.Attributes    
open BenchmarkDotNet.Attributes.Jobs    
open BenchmarkDotNet.Running
    
[<MemoryDiagnoser>]
//[<ClrJob; CoreJob>]    
type Test() =
    [<Benchmark(Baseline = true)>]
    member __.Base() = Base.jaro "Environment" "Envronment"
    
    [<Benchmark>]
    member __.Opt() = Opt.jaro ("Environment", "Envronment", false)
    
    [<Benchmark>]
    member __.OptArrayPool() = Opt.jaro ("Environment", "Envronment", true)        
    
open System

[<EntryPoint>]
let main argv =
    printfn "%f" (Opt.jaro ("Environment", "Envronment", true))
    BenchmarkRunner.Run<Test>() |> ignore
//    let sw = System.Diagnostics.Stopwatch.StartNew()
//    for i = 1 to 10_000_000 do
//        Opt.jaro "Environment" "Envronment" |> ignore
//    printfn "%O" sw.Elapsed
    0
    