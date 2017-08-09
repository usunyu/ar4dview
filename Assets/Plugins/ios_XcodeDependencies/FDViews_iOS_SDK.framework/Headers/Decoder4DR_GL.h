//
//  Decoder4DR_GL.h
//
//  Copyright 2013 4D View Solutions. All rights reserved.
//

#import <Foundation/Foundation.h>
#import <GLKit/GLKit.h>
#import "Decoder4DR.h"

@interface Decoder4DR_GL : NSObject
@property (nonatomic, readonly) BOOL shadowEnabled;

//Constructor
//Set YES to shadow for also initializing the object for shadow rendering
//GL context must be set before calling this method
- (id) initWithShadowEnabled:(BOOL)shadow;

//Sets a new mesh decoder
//GL context must be set before calling this method
- (void) update:(Decoder4DR*)decoder;

//Draws the current mesh decoder (very optimized!)
//GL context must be set before calling this method
- (void) draw:(GLKMatrix4)MVPMatrix;

//Draws the current mesh decoder for shadow  (very optimized!)
//GL context must be set before calling this method
- (void) drawShadow:(GLKMatrix4)MVPMatrix;

//Custom shaders can be defined for not using the default integrated ones
//Set 0 for disabling custom shaders and using default shaders
//WARNING: custom shaders must contains the default shaders attributes and uniforms
// defined below
- (void) setCustomColorShaderProgram:(GLuint) programId;
- (void) setCustomShadowShaderProgram:(GLuint) programId;

@end


#define DEFAULT_COLOR_ATTRIBS  @"aPosition", @"aUVCoord"
#define DEFAULT_SHADOW_ATTRIBS @"aPosition"

#define DEFAULT_COLOR_UNIFORMS  @"uMVPMatrix",@"uQuantizerMin",@"uQuantizerScale",@"uQuantizerUVScale",@"uTexture"
#define DEFAULT_SHADOW_UNIFORMS @"uMVPMatrix",@"uQuantizerMin",@"uQuantizerScale"


#if TARGET_OS_IPHONE
#define DEFAULT_COLOR_VERTEX_SHADER  @"" \
"precision mediump float;" \
"attribute vec4 aPosition;" \
"attribute vec2 aUVCoord;" \
"uniform   mat4 uMVPMatrix;" \
"uniform   vec3 uQuantizerMin;" \
"uniform   vec3 uQuantizerScale;" \
"uniform   vec2 uQuantizerUVScale;" \
"varying   vec2 vTexCoordOut;" \
"void main()" \
"{" \
"    vTexCoordOut = aUVCoord*uQuantizerUVScale;" \
"    vec4 pos;" \
"    pos.xyz = uQuantizerMin + aPosition.xyz * uQuantizerScale;" \
"    pos.w = aPosition.w;" \
"    gl_Position =  uMVPMatrix * pos;" \
"}"

#define DEFAULT_COLOR_FRAGMENT_SHADER  @"" \
"precision mediump float;" \
"uniform lowp sampler2D uTexture;" \
"varying vec2 vTexCoordOut;" \
"void main()" \
"{" \
"    gl_FragColor = texture2D(uTexture, vTexCoordOut);" \
"}"


#define DEFAULT_SHADOW_VERTEX_SHADER  @"" \
"precision mediump float;" \
"attribute vec4 aPosition;" \
"uniform mat4 uMVPMatrix;" \
"uniform vec3 uQuantizerMin;" \
"uniform vec3 uQuantizerScale;" \
"void main()" \
"{" \
"    vec4 pos;" \
"    pos.xyz = uQuantizerMin + aPosition.xyz * uQuantizerScale;" \
"    pos.w = aPosition.w;" \
"    gl_Position =  uMVPMatrix * pos;" \
"}"

#define DEFAULT_SHADOW_FRAGMENT_SHADER  @"" \
"precision mediump float;" \
"void main()" \
"{" \
"    gl_FragColor.rgb = vec3(gl_FragCoord.z);" \
"    gl_FragColor.a = 1.;" \
"}"

#else

#define DEFAULT_COLOR_VERTEX_SHADER  @"" \
"#version 150 core\n" \
"in vec4 aPosition;" \
"in vec2 aUVCoord;" \
"uniform   mat4 uMVPMatrix;" \
"uniform   vec3 uQuantizerMin;" \
"uniform   vec3 uQuantizerScale;" \
"uniform   vec2 uQuantizerUVScale;" \
"out   vec2 vTexCoordOut;" \
"void main()" \
"{" \
"    vTexCoordOut = aUVCoord*uQuantizerUVScale;" \
"    vec4 pos;" \
"    pos.xyz = uQuantizerMin + aPosition.xyz * uQuantizerScale;" \
"    pos.w = aPosition.w;" \
"    gl_Position =  uMVPMatrix * pos;" \
"}"

#define DEFAULT_COLOR_FRAGMENT_SHADER  @"" \
"#version 150 core\n" \
"uniform sampler2D uTexture;" \
"in vec2 vTexCoordOut;" \
"out vec4 color;" \
"void main()" \
"{" \
"    color = texture(uTexture, vTexCoordOut);" \
"}"


#define DEFAULT_SHADOW_VERTEX_SHADER  @"" \
"#version 150 core\n" \
"in vec4 aPosition;" \
"uniform mat4 uMVPMatrix;" \
"uniform vec3 uQuantizerMin;" \
"uniform vec3 uQuantizerScale;" \
"void main()" \
"{" \
"    vec4 pos;" \
"    pos.xyz = uQuantizerMin + aPosition.xyz * uQuantizerScale;" \
"    pos.w = aPosition.w;" \
"    gl_Position =  uMVPMatrix * pos;" \
"}"

#define DEFAULT_SHADOW_FRAGMENT_SHADER  @"" \
"#version 150 core\n" \
"out vec4 color;" \
"void main()" \
"{" \
"    color = vec4(1.);" \
"}"


#endif

