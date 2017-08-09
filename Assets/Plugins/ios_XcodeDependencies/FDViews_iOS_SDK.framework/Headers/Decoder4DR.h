//
//  Decoder4DR.h
//
//  Copyright 2013 4D View Solutions. All rights reserved.
//

#import <Foundation/Foundation.h>
#import "Mesh4DR.h"
#import "Texture4DR.h"


@interface Decoder4DR : NSObject

//----- Constructors

//init and decode from the file, device, or named socket at the specified URL
- (id) initFromURL:(NSURL*)dataURL byDecompressingGeometry:(BOOL)val;
//init and decode from the raw data buffer
- (id) initFromData:(NSData*)dataBuffer byDecompressingGeometry:(BOOL)val;


//------ Geometry and Texture Getters

@property (readonly, retain) Mesh4DR* m_mesh;
@property (readonly, retain) Texture4DR* m_texture;



@end
