#r "System.Xml.Linq.dll"
#r "packages/FSharp.Data.2.0.0-alpha6/lib/net40/FSharp.Data.dll"
open FSharp.Data
open System

let toString (cs:seq<char>) = String(cs |> Seq.toArray)

// ------------------------------------------------------------------
// WORD #1
//
// Use the WorldBank type provider to get all countries in the 
// "North America" region, then find country "c" with the smallest
// "Life expectancy at birth, total (years)" value in the year 2000
// and return the first two letters of the "c.Code" property
// ------------------------------------------------------------------

// Create connection to the WorldBank service
let wb = WorldBankData.GetDataContext()
// Get specific indicator for a specific country at a given year
wb.Countries.``Czech Republic``.Indicators.``Population (Total)``.[2000]
// Get a list of countries in a specified region
let word1 = wb.Regions.``North America``.Countries 
            |> Seq.minBy (fun c -> c.Indicators.``Life expectancy at birth, total (years)``.GetValueAtOrZero 2000)
            |> (fun c -> c.Code)
            |> Seq.take 2 
            |> toString
    

// ------------------------------------------------------------------
// WORD #2
//
// Read the RSS feed in "data/bbc.xml" using XmlProvider and return
// the last word of the title of an article that was published at
// 9:05am (the date does not matter, just hours & minutes)
// ------------------------------------------------------------------

// Create a type for working with XML documents based on a sample file
type Sample = XmlProvider<"data/bbc.xml">
// Load the sample document - explore properties using "doc."
let doc = Sample.GetSample()
let item = doc.Channel.GetItems() |> Seq.tryFind ( fun i -> i.PubDate.Hour = 9 && i.PubDate.Minute = 5)
let word2 = item.Value.Title.Split ' ' |> Seq.last

// ------------------------------------------------------------------
// WORD #3
//
// Get the ID of a director whose name contains "Haugerud" and then
// search for all movie credits where he appears. Then return the 
// second (last) word from the movie he directed (the resulting type
// has properties "Credits" and "Crew" - you need movie from the 
// Crew list (there is only one).
// ------------------------------------------------------------------

// Using The MovieDB REST API
// Make HTTP request to /3/search/person
let key = "6ce0ef5b176501f8c07c634dfa933cff"
let name = "Haugerud"
let searchResponse = 
  Http.Request
    ( "http://api.themoviedb.org/3/search/person",
      query = [ ("query", name); ("api_key", key) ],
      headers = ["accept", "application/json"] )

let data = 
        searchResponse.Body 
        |> function 
            | ResponseBody.Text t-> t
            | _ -> failwith "the response body should be a text"

// Parse result using JSON provider
// (using sample result to generate types)
type PersonSearch = JsonProvider<"data/personsearch.json">
let personSearch = PersonSearch.Parse(data)

let first = personSearch.Results |> Seq.head
//first.Id //first.Name

// Request URL: "http://api.themoviedb.org/3/person/<id>/movie_credits
// (You can remove the 'query' parameter because it is not needed here;
// you need to put the director's ID in place of the <id> part of the URL)

// Use JsonProvider with sample file "data/moviecredits.json" to parse
let creditsResponse = 
    Http.Request
        ( sprintf "http://api.themoviedb.org/3/person/%d/movie_credits" first.Id,
          query = [("api_key", key) ],
          headers = ["accept", "application/json"] )

let creditsResponseBody = 
        creditsResponse.Body
        |> function
            | ResponseBody.Text t -> t
            | _ -> failwith "the response body should be a text"

type MovieCredits = JsonProvider<"data/moviecredits.json">
let creditsSearch = MovieCredits.Parse(creditsResponseBody)

let movie = creditsSearch.Crew |> Seq.filter (fun c -> c.Job = "Director") |> Seq.head
let word3 = movie.Title.Split ' ' |> Seq.last


// ------------------------------------------------------------------
// WORD #4
//
// Use CsvProvider to parse the file "data/librarycalls.csv" which
// contains information about some PHP library calls (got it from the
// internet :-)). Note that the file uses ; as the separator.
//
// Then find row where 'params' is equal to 2 and 'count' is equal to 1
// and the 'name' column is longer than 6 characters. Return first such
// row and get the last word of the function name (which is separated
// by underscores). Make the word plural by adding 's' to the end.
// ------------------------------------------------------------------

type Calls = CsvProvider<"data/librarycalls.csv", Separators=";">
let calls = new Calls()
let foundRow = calls.Rows 
                |> Seq.find (fun r -> r.``params`` = 2 && r.count = 1 && r.name.Length > 6)
let word4 = (foundRow.name.Split '_' |> Seq.last) + "s"

//
//// Demo - using CSV provider to read CSV file with custom separator
//type Lib = CsvProvider<"data/mortalityny.tsv", Separators="\t">
//// Read the sample file - explore the collection of rows in "lib.Data"
//let lib = new Lib()
//lib.Rows |> Seq.take 30 |> Seq.toList

// ------------------------------------------------------------------
// WORD #5
//
// Use Freebase type provider to find chemical element with 
// "Atomic number" equal to 36 and then return the 3rd and 2nd 
// letter from the *end* of its name (that is 5th and 6th letter
// from the start).
// ------------------------------------------------------------------

// Create connection to the Freebase service
let fb = FreebaseData.GetDataContext()

// Query stars in the science & technology category
// (You'll need "Science and Technology" and then "Chemistry")
let word5 = 
    query { for e in fb.``Science and Technology``.Chemistry.``Chemical Elements`` do 
            where (e.``Atomic number``.GetValueOrDefault() = 36)
            select e.Name } 
    |> Seq.head 
    |> Seq.skip 4 |> Seq.take 2
    |> toString


// ------------------------------------------------------------------
// WORD #6
//
// Use CsvProvider to load the Titanic data set from "data/Titanic.csv"
// (using the default column separator) and find the name of a female
// passenger who was 19 years old and embarked in the prot marked "Q"
// Then return Substring(19, 3) from her name.
// ------------------------------------------------------------------

// Use CsvProvider with the "data/titanic.csv" file as a sample
type TitanicData = CsvProvider<"data/Titanic.csv">
let titanic = new TitanicData()
//[for r in titanic.Rows -> (r.Age, r.Embarked)]
let word6 = (titanic.Rows
            |> Seq.find (fun r -> r.Age = 19. && r.Embarked = "Q")).Name.Substring(19,3)


// ------------------------------------------------------------------
// WORD #7
//
// Using the same RSS feed as in Word #2 ("data/bbc.xml"), find
// item that contains "Duran Duran" in the title and return the
// 14th word from its description (split the description using ' '
// as the separator and get item at index 13).
// ------------------------------------------------------------------

// (...)

[word1;word2;word3;word4;word5;word6]
