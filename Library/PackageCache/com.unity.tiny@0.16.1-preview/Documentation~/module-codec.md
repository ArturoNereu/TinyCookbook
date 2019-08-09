# Codec Module

The Codec module provides a simple API for compressing and decompressing data with a handful of supported codecs. 

For scene data, compression is handled automatically by scene module, so for most users the codec module does not need to be used directly. However if your game requires compressing/decompressing game data at runtime then the `CodecService` API provides simple functions for non-streaming compression/decompression.

(See this module's API documentation for more information)