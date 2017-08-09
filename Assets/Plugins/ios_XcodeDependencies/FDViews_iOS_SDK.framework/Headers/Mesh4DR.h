//
//  Mesh4DR.h
//
//  Copyright 2013 4D View Solutions. All rights reserved.
//

#import <Foundation/Foundation.h>

//Quantizer allows to decompress vertices and uvs buffers
//if the decoder did not decompress the geometry
typedef struct {
    float xMin;
    float xScale;
    float yMin;
    float yScale;
    float zMin;
    float zScale;
    float uvXScale;
    float uvYScale;
    float precision;
} Quantizer;


typedef struct {
    float x;
    float y;
    float z;
} VertexPosition;

typedef struct {
    float u;
    float v;
} VertexUV;

typedef struct {
    VertexPosition position;
    VertexUV     coordinates;
} InterleavedVertexPositionUV;


typedef struct {
    unsigned short x;
    unsigned short y;
    unsigned short z;
    unsigned short padding_for_alignment;
} CompressedVertexPosition;

typedef struct {
    unsigned short u;
    unsigned short v;
} CompressedVertexUV;

typedef struct {
    CompressedVertexPosition position;
    CompressedVertexUV     coordinates;
} InterleavedCompressedVertexPositionUV;


typedef struct {
    unsigned short i1;
    unsigned short i2;
    unsigned short i3;
} ShortFace;

typedef struct {
    unsigned int i1;
    unsigned int i2;
    unsigned int i3;
} IntFace;


typedef struct {
    float x;
    float y;
    float z;
} Normal;




@interface Mesh4DR : NSObject


@property int m_nbVertices;
@property int m_nbFaces;


@property BOOL isGeometryUncompressed;              // YES if decoder did decompress the geometry
                                                    // (NO for later decompression by a vertex shader for ex.)

@property BOOL isGeometryInterleaved;               // YES if vertex positions and coordinates
                                                    // are interleaved

@property BOOL containsNormals;                     // YES if vertex normals are present


//--- Uncompressed geometry getters
@property VertexPosition* m_uncompressedVertices;          // NULL if compressed or interleaved
@property VertexUV* m_uncompressedUVCoords;                // NULL if compressed or interleaved
@property InterleavedVertexPositionUV* m_uncompressedInterleavedVerticesUVCoords; //NULL if compressed
                                                                            //or not interleaved
@property Normal* m_uncompressedNormals;

//--- Compressed geometry getters
@property Quantizer m_quantizer;
@property CompressedVertexPosition* m_compressedVertices;  // NULL if uncompressed
@property CompressedVertexUV* m_compressedUVCoords;        // NULL if uncompressed
@property InterleavedCompressedVertexPositionUV* m_compressedInterleavedVerticesUVCoords; //NULL if uncompressed
                                                                                          //or not interleaved
@property Normal* m_compressedNormals;



//--- Triangles getters
@property ShortFace* m_sfaces;                     // if nbVertices ≤ 65535 else NULL
@property IntFace* m_ifaces;                       // if nbVertices > 65535 else NULL


@end
