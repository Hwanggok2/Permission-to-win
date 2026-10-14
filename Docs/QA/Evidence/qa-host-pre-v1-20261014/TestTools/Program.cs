using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using StageGame;
using UnityEngine;

static class Program
{
    static readonly List<object> Results = new List<object>();
    static int passed, failed, assertions;
    static void Check(bool ok, string message) { assertions++; if(!ok) throw new Exception(message); }
    static void Run(string name, Action test)
    {
        try { test(); passed++; Results.Add(new{name,status="PASS",error=(string)null}); Console.WriteLine("PASS "+name); }
        catch(Exception e) { failed++; Results.Add(new{name,status="FAIL",error=e.Message}); Console.WriteLine("FAIL "+name+" | "+e.Message); }
    }
    static void Equal(Rect actual, float x, float y, float width, float height)
    {
        Check(float.IsFinite(actual.x) && float.IsFinite(actual.y) && float.IsFinite(actual.width) && float.IsFinite(actual.height), "non-finite rectangle");
        Check(Math.Abs(actual.x-x)<0.002 && Math.Abs(actual.y-y)<0.002 && Math.Abs(actual.width-width)<0.002 && Math.Abs(actual.height-height)<0.002,
            $"expected ({x},{y},{width},{height}); actual ({actual.x},{actual.y},{actual.width},{actual.height})");
    }
    static Rect Area(int w,int h,Vector4 inset,float controls=-1) => HostLayout.ContentArea(new Rect(0,0,w,h),new Vector2Int(w,h),new Vector2(w,h),inset,controls);
    static int Main(string[] args)
    {
        Run("host.no_notch_64_once_320_390",()=> {
            Equal(Area(320,568,new Vector4()),0,0,320,504);
            Equal(Area(390,844,new Vector4()),0,0,390,780);
        });
        Run("host.same_os_and_css_notch_not_added_twice",()=> {
            Equal(HostLayout.ContentArea(new Rect(20,68,748,1532),new Vector2Int(780,1688),new Vector2(390,844),new Vector4(10,44,6,34)),20,68,748,1404);
        });
        Run("host.max_each_edge_and_asymmetric_pixel_ratio",()=> {
            Equal(HostLayout.ContentArea(new Rect(40,80,580,956),new Vector2Int(640,1136),new Vector2(320,568),new Vector4(10,44,6,34)),40,80,580,828);
            Equal(HostLayout.ContentArea(new Rect(30,68,1122,1532),new Vector2Int(1170,1688),new Vector2(390,844),new Vector4(10,44,6,34)),30,68,1122,1404);
        });
        Run("host.controls_bottom_union_not_another_addition",()=> {
            Equal(Area(390,844,new Vector4(0,44,0,34),150),0,34,390,652);
            Equal(Area(390,844,new Vector4(0,44,0,34),90),0,34,390,702);
            Equal(Area(390,844,new Vector4(),0),0,0,390,780);
        });
        Run("host.finite_extremes_zero_area_and_negative_inputs",()=> {
            Equal(Area(320,568,new Vector4(-5,-7,-9,-11)),0,0,320,504);
            var zero=Area(320,568,new Vector4(1000,1000,1000,1000));
            Check(zero.width==0 && zero.height==0 && zero.x>=0 && zero.x<=320 && zero.y>=0 && zero.y<=568,"overlarge insets must produce bounded empty area");
            Equal(Area(320,568,new Vector4(),1000),0,0,320,0);
            Equal(HostLayout.ContentArea(new Rect(0,0,320,568),new Vector2Int(320,568),new Vector2(0,568),new Vector4()),0,0,320,568);
        });
        Run("host.css_to_float32_overflow_stays_finite",()=> {
            // A finite JavaScript Number (1e300) becomes +Infinity in HEAPF32.
            var actual=Area(320,568,new Vector4(float.PositiveInfinity,float.PositiveInfinity,float.PositiveInfinity,float.PositiveInfinity));
            Check(float.IsFinite(actual.x) && float.IsFinite(actual.y) && float.IsFinite(actual.width) && float.IsFinite(actual.height),"float32 overflow reaches non-finite output");
            var nan=Area(320,568,new Vector4(float.NaN,float.NaN,float.NaN,float.NaN));
            Check(float.IsFinite(nan.x) && float.IsFinite(nan.y) && float.IsFinite(nan.width) && float.IsFinite(nan.height),"NaN reaches content anchors");
        });
        Run("host.portrait_resize_is_stateless",()=> {
            Equal(Area(390,844,new Vector4(0,44,0,34)),0,34,390,702);
            Equal(Area(320,568,new Vector4(0,44,0,34)),0,34,320,426);
            Equal(Area(390,844,new Vector4(0,44,0,34)),0,34,390,702);
        });
        File.WriteAllText(args[0],JsonSerializer.Serialize(new{passed,failed,assertions,results=Results,
            limits="Actual HostLayout source; numeric Unity API doubles only. Zero-area result does not prove UI is hidden. Rotation usability, hitbox/rendering and device behavior are not executed."},new JsonSerializerOptions{WriteIndented=true}));
        return failed==0?0:1;
    }
}
