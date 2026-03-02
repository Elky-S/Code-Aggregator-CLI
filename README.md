# 🛠️ Code Aggregator CLI

**A high-performance C# tool to bundle source code files into a single organized package.**

---

## 🚀 Commands

### 📦 `bundle`
The main command to package your code. 
* **`-l, --language`**: Select languages (e.g., `cs`, `py`, `all`). [Required]
* **`-o, --output`**: Set destination path or filename.
* **`-n, --note`**: Include source file headers (path and name).
* **`-s, --sort`**: Order by `name` (alphabetical) or `type` (extension).
* **`-r, --remove-empty-lines`**: Strip empty lines from source.
* **`-a, --author`**: Add the creator's name to the file header.

### ✍️ `create-rsp`
An interactive wizard that asks questions and generates a **Response File** (`.rsp`) with your preferred configuration. This avoids the need to type long commands repeatedly.

## 💡 Smart Filtering
* **Recursive Search**: Automatically traverses all subdirectories.
* **Auto-Ignore**: Intelligent filtering that skips `bin`, `debug`, and `obj` folders to keep the bundle clean.

## 💻 Installation & Usage

### 1. Installation
* **Build**: Run `dotnet build` in the project directory.
* **Global Access**: Add the `/publish` folder path to your system **Environment Variables (PATH)** to run the tool from any location.

### 2. Usage Example
To run the tool using a response file:
```bash
dotnet @mybundle.rsp
```
## 📚 References
This project was developed following the official Microsoft documentation for **System.CommandLine**:
* [Microsoft Learn - Command-line syntax](https://learn.microsoft.com/en-us/dotnet/standard/commandline/syntax)

---
Built using **System.CommandLine** (.NET 8).