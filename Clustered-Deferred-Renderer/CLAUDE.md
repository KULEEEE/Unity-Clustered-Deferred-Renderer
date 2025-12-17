# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

This is a Unity 6 (6000.2.10f1) project implementing a custom **Clustered Deferred Renderer** using Unity's Scriptable Render Pipeline (SRP) and the RenderGraph API.

## Build and Development

This is a Unity project - open in Unity Editor to build and test. There is no command-line build configured.

- **Unity Version**: 6000.2.10f1
- **Key Package**: `com.unity.render-pipelines.core` (17.2.0) - provides SRP foundation

## Architecture

### Custom Render Pipeline

The renderer is implemented in `Assets/ClusteredRenderer/`:

- **Runtime/**
  - `ClusteredRenderPipelineAsset.cs` - ScriptableObject that creates pipeline instances. Assigned in Project Settings > Graphics to activate the pipeline
  - `ClusteredRenderPipeline.cs` - Core pipeline using RenderGraph API. Implements `RenderPipeline.Render()` to process cameras

- **Shaders/**
  - Custom HLSL shaders using `Tags { "LightMode" = "ExampleLightModeTag" }` for SRP compatibility

- **Editor/**
  - Contains the pipeline asset instance used by the project

### Render Pipeline Flow

1. `ClusteredRenderPipelineAsset.CreatePipeline()` instantiates the pipeline
2. `ClusteredRenderPipeline.Render()` iterates enabled cameras
3. `RenderCamera()` uses RenderGraph to record and execute render passes
4. Passes use `IRasterRenderPass` pattern with `PassData` structs for state

### Key APIs

- `RenderGraph` - Modern Unity rendering graph for pass management
- `ScriptableRenderContext` - Interface to Unity's rendering backend
- `CommandBuffer` / `CommandBufferPool` - GPU command recording

## Conventions

- Shaders use HLSL program blocks (`HLSLPROGRAM`/`ENDHLSL`)
- Pass tags must match `ShaderTagId` used in draw calls
- Pipeline asset lives in Editor/ folder, referenced by Graphics Settings
