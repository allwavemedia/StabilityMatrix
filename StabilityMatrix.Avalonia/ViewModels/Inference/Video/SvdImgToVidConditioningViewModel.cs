﻿using System.ComponentModel.DataAnnotations;
using CommunityToolkit.Mvvm.ComponentModel;
using StabilityMatrix.Avalonia.Controls;
using StabilityMatrix.Avalonia.Models;
using StabilityMatrix.Avalonia.Models.Inference;
using StabilityMatrix.Avalonia.ViewModels.Base;
using StabilityMatrix.Core.Attributes;
using StabilityMatrix.Core.Models;
using StabilityMatrix.Core.Models.Api.Comfy.Nodes;

namespace StabilityMatrix.Avalonia.ViewModels.Inference.Video;

[View(typeof(VideoGenerationSettingsCard))]
[ManagedService]
[Transient]
public partial class SvdImgToVidConditioningViewModel
    : LoadableViewModelBase,
        IParametersLoadableState,
        IComfyStep
{
    [ObservableProperty]
    private int width = 1024;

    [ObservableProperty]
    private int height = 576;

    [ObservableProperty]
    private int numFrames = 14;

    [ObservableProperty]
    private int motionBucketId = 127;

    [ObservableProperty]
    private int fps = 6;

    [ObservableProperty]
    private double augmentationLevel;

    [ObservableProperty]
    private double minCfg = 1.0d;

    public void LoadStateFromParameters(GenerationParameters parameters)
    {
        // TODO
    }

    public GenerationParameters SaveStateToParameters(GenerationParameters parameters)
    {
        // TODO
        return parameters;
    }

    public void ApplyStep(ModuleApplyStepEventArgs e)
    {
        // do VideoLinearCFGGuidance stuff first
        var cfgGuidanceNode = e.Nodes.AddTypedNode(
            new ComfyNodeBuilder.VideoLinearCFGGuidance
            {
                Name = e.Nodes.GetUniqueName("LinearCfgGuidance"),
                Model =
                    e.Builder.Connections.BaseModel
                    ?? throw new ValidationException("Model not selected"),
                MinCfg = MinCfg
            }
        );

        e.Builder.Connections.BaseModel = cfgGuidanceNode.Output;

        // then do the SVD stuff
        var svdImgToVidConditioningNode = e.Nodes.AddTypedNode(
            new ComfyNodeBuilder.SVD_img2vid_Conditioning
            {
                ClipVision = e.Builder.Connections.BaseClipVision!,
                InitImage = e.Builder.GetPrimaryAsImage(),
                Vae = e.Builder.Connections.BaseVAE!,
                Name = e.Nodes.GetUniqueName("SvdImgToVidConditioning"),
                Width = Width,
                Height = Height,
                VideoFrames = NumFrames,
                MotionBucketId = MotionBucketId,
                Fps = Fps,
                AugmentationLevel = AugmentationLevel
            }
        );

        e.Builder.Connections.BaseConditioning = svdImgToVidConditioningNode.Output1;
        e.Builder.Connections.BaseNegativeConditioning = svdImgToVidConditioningNode.Output2;
        e.Builder.Connections.Primary = svdImgToVidConditioningNode.Output3;
    }
}
