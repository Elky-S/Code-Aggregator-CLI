# 🛠️ Code Aggregator CLI
**A high-performance C# CLI tool for automated code file aggregation, featuring advanced filtering and output management.**

This tool was designed to help developers quickly gather source code from multiple files into a single document, making it easier to share, review, or feed into LLMs (Large Language Models) for analysis.

## 🚀 Key Features
* **Recursive Search: Automatically traverses through subdirectories.**

* **Smart Filtering: Includes/Excludes files based on extensions (e.g., .cs, .py, .js).**

Clean Output: Merges code into a formatted file with clear headers indicating the source of each snippet.

Performance: Built on .NET 8 for speed and reliability.

## 💻 How to Use
**Clone the repository:**
git clone https://github.com/Elky-S/Code-Aggregator-CLI.git
cd Code-Aggregator-CLI/cli
**Run the application:**
Use the dotnet run command followed by your parameters:
dotnet run -- --root "C:/Path/To/Your/Project" --output "merged_code.txt"
## 🛠️ Built With
C# / .NET 8

System.CommandLine - For robust CLI argument parsing.

## 📄 License
This project is open-source and available under the MIT License.