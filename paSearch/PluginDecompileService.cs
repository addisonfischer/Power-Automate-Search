using ICSharpCode.Decompiler;
using ICSharpCode.Decompiler.CSharp;
using ICSharpCode.Decompiler.TypeSystem;
using System;
using System.Linq;
using System.Collections.Concurrent;
using System.IO;

namespace paSearch
{
    /// <summary>
    /// Service for decompiling plugin assemblies with caching support
    /// Enables searching through plugin source code
    /// </summary>
    public class PluginDecompilationService
    {
        // Cache decompiled code per session (assemblyId -> decompiled code)
        private static readonly ConcurrentDictionary<Guid, DecompiledAssembly> _decompiledCodeCache = 
            new ConcurrentDictionary<Guid, DecompiledAssembly>();
        
        /// <summary>
        /// Decompile a plugin assembly from Dataverse
        /// </summary>
        /// <param name="assemblyBytes">The binary content of the assembly</param>
        /// <param name="assemblyId">Unique ID for caching</param>
        /// <param name="assemblyName">Name of the assembly (for logging)</param>
        /// <param name="useCache">Whether to use cached results</param>
        /// <returns>Decompiled code or error message</returns>
        public DecompiledAssembly DecompilePlugin(byte[] assemblyBytes, Guid assemblyId, string assemblyName, bool useCache = true)
        {
            // Check cache first
            if (useCache && _decompiledCodeCache.TryGetValue(assemblyId, out DecompiledAssembly cachedCode))
            {
                return cachedCode;
            }
            
            var result = new DecompiledAssembly
            {
                AssemblyId = assemblyId,
                AssemblyName = assemblyName,
                Success = false
            };
            
            string tempPath = null;
            
            try
            {
                // Write to temp file (decompiler requires file path)
                tempPath = Path.Combine(Path.GetTempPath(), $"paSearch_{assemblyId}.dll");
                File.WriteAllBytes(tempPath, assemblyBytes);
                
                // Configure decompiler settings
                var decompilerSettings = new DecompilerSettings
                {
                    ThrowOnAssemblyResolveErrors = false,
                    // Use modern C# features for better readability
                    UseDebugSymbols = false,
                    // Don't fail on missing dependencies
                    LoadInMemory = true
                };
                
                // Create decompiler
                var decompiler = new CSharpDecompiler(tempPath, decompilerSettings);
                
                // Decompile entire module to string
                result.DecompiledCode = decompiler.DecompileWholeModuleAsString();
                result.Success = true;
                result.TypeCount = decompiler.TypeSystem.MainModule.TypeDefinitions.Count();
                
                // Cache the result
                if (useCache)
                {
                    _decompiledCodeCache.TryAdd(assemblyId, result);
                }
            }
            catch (BadImageFormatException ex)
            {
                result.ErrorMessage = $"Invalid assembly format: {ex.Message}";
                result.DecompiledCode = "/* Decompilation failed: Not a valid .NET assembly */";
            }
            catch (FileNotFoundException ex)
            {
                result.ErrorMessage = $"Missing dependencies: {ex.Message}";
                result.DecompiledCode = "/* Decompilation failed: Missing dependencies - assembly may use external DLLs not available */";
            }
            catch (PlatformNotSupportedException ex)
            {
                result.ErrorMessage = $"Unsupported platform: {ex.Message}";
                result.DecompiledCode = "/* Decompilation failed: Unsupported assembly format (may be .NET Core/.NET 5+) */";
            }
            catch (Exception ex)
            {
                result.ErrorMessage = $"Unexpected error: {ex.Message}";
                result.DecompiledCode = $"/* Decompilation failed: {ex.Message} */";
            }
            finally
            {
                // Clean up temp file
                try
                {
                    if (tempPath != null && File.Exists(tempPath))
                    {
                        File.Delete(tempPath);
                    }
                }
                catch
                {
                    // Ignore cleanup errors
                }
            }
            
            return result;
        }
        
        /// <summary>
        /// Clear the decompilation cache
        /// </summary>
        public void ClearCache()
        {
            _decompiledCodeCache.Clear();
        }
        
        /// <summary>
        /// Get cache statistics
        /// </summary>
        public CacheStatistics GetCacheStatistics()
        {
            return new CacheStatistics
            {
                CachedAssemblies = _decompiledCodeCache.Count,
                TotalTypes = 0 // Could calculate if needed
            };
        }
    }
    
    /// <summary>
    /// Result of decompilation operation
    /// </summary>
    public class DecompiledAssembly
    {
        public Guid AssemblyId { get; set; }
        public string AssemblyName { get; set; }
        public bool Success { get; set; }
        public string DecompiledCode { get; set; }
        public string ErrorMessage { get; set; }
        public int TypeCount { get; set; }
    }
    
    /// <summary>
    /// Cache statistics for monitoring
    /// </summary>
    public class CacheStatistics
    {
        public int CachedAssemblies { get; set; }
        public int TotalTypes { get; set; }
    }
}
