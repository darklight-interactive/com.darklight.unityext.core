# Changelog

## [0.2.1] - 2024-12-19
### Added
- **JsonDataService**: Complete JSON-based data persistence system with AES encryption support
  - `IDataService` interface for standardized data operations
  - `SaveData<T>()` and `LoadData<T>()` methods with optional encryption
  - Automatic file management with overwrite protection
  - Comprehensive error handling and logging
  - AES-256 encryption with configurable keys and initialization vectors

- **Library System**: Advanced key-value data management framework
  - `Library<TKey, TValue>` base class with full IDictionary implementation
  - `ScriptableLibrary<TKey, TValue, TLib>` for Unity ScriptableObject integration
  - `LibraryItem<TKey, TValue>` for structured data storage
  - Support for read-only keys and values
  - Required keys validation system
  - Event-driven architecture with `ItemAdded` and `ItemRemoved` events

- **Collection System**: Thread-safe collection management
  - `CollectionLibrary<TValue>` with concurrent dictionary support
  - `CollectionHash<TValue>` with SHA256 integrity verification
  - `CollectionItem<TValue>` and `KeyValueCollectionItem<TKey, TValue>`
  - Thread-safe operations with ReaderWriterLockSlim
  - Pagination and search capabilities via `CollectionGuiSettings`
  - Batch operations support (ADD, REMOVE, CLEAR, UPDATE, REPLACE, RESET, SORT)

- **ScriptableData System**: Unity ScriptableObject-based data persistence
  - Custom editor integration with `ScriptableDataCustomEditor`
  - Property drawer support for easy asset creation
  - Automatic asset creation workflow with user-friendly dialogs
  - Type-safe data storage with generic constraints
  - Editor-only asset management utilities

- **Specialized Library Types**:
  - `EnumKeyLibrary<TKey, TValue>` for enum-based key management
  - `IntKeyLibrary<TValue>` and `StringKeyLibrary<TValue>` for common key types
  - `EnumGameObjectScriptableLibrary<TKey>` for GameObject references

- **Data Utilities**:
  - `WeightedDataSelector` for weighted random selection algorithms
  - `Context` system for AI data management in RemixSurvivors
  - Matrix-based data organization with `MatrixMap`

### Changed
- Enhanced data persistence with encryption capabilities
- Improved thread safety across all collection operations
- Streamlined ScriptableObject integration for data assets
- Optimized memory usage with concurrent collections

### Fixed
- Thread safety issues in collection operations
- File handling edge cases in JsonDataService
- Asset creation workflow in ScriptableData system
- Memory leaks in collection event handling

### Removed
- Legacy data storage methods (replaced by JsonDataService)
- Deprecated collection implementations