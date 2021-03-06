﻿using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// Custom editor for Rule Tiles.
/// </summary>
[CustomEditor(typeof(RuleTile))]
[CanEditMultipleObjects]
public class RuleTileEditor : Editor
{
    private static Texture blackCircle;
    private static Texture greenCheckmark;
    private static Texture redX;
    private static Texture blueBox;

    private ReorderableList reorderableList;
    public RuleTile tile { get { return (target as RuleTile); } }

    private const float defaultElementHeight = 48.0f;
    private const float paddingBetweenRules = 13.0f;
    private const float singleLineHeight = 16.0f;
    private const float labelWidth = 53.0f;

    private const string blackCirclePath = "Assets/Sprites/Editor/rule-tile.png";
    private const string greenCheckmarkPath = "Assets/Sprites/Editor/rule-tile-this.png";
    private const string redXPath = "Assets/Sprites/Editor/rule-tile-not_this.png";
    private const string blueBoxPath = "Assets/Sprites/Editor/rule-tile-another_tile.png";

    public void OnEnable()
    {
        blackCircle = (Texture) AssetDatabase.LoadAssetAtPath(blackCirclePath, typeof (Texture));
        greenCheckmark = (Texture)AssetDatabase.LoadAssetAtPath(greenCheckmarkPath, typeof(Texture));
        redX = (Texture)AssetDatabase.LoadAssetAtPath(redXPath, typeof(Texture));
        blueBox = (Texture)AssetDatabase.LoadAssetAtPath(blueBoxPath, typeof(Texture));

        if (tile.tilingRules == null)
        {
            tile.tilingRules = new List<RuleTile.TilingRule>();
        }

        reorderableList = new ReorderableList(tile.tilingRules, typeof(RuleTile.TilingRule), true, true, true, true);
        reorderableList.drawHeaderCallback = OnDrawHeader;
        reorderableList.drawElementCallback = OnDrawElement;
        reorderableList.elementHeightCallback = GetElementHeight;
        reorderableList.onReorderCallback = ListUpdated;
        reorderableList.onAddCallback = OnAdd;
    }

    /// <summary>
    /// Method to draw custom editor header.
    /// </summary>
    private void OnDrawHeader(Rect rect)
    {
        GUI.Label(rect, "Tiling Rules");
    }

    /// <summary>
    /// Method to draw custom editor body.
    /// </summary>
    private void OnDrawElement(Rect rect, int index, bool isActive, bool isFocused)
    {
        RuleTile.TilingRule rule = tile.tilingRules[index];

        float yPos = rect.yMin + 2.0f;
        float height = rect.height - paddingBetweenRules;
        float matrixWidth = defaultElementHeight;

        Rect inspectorRect = new Rect(rect.xMin, yPos, rect.width - matrixWidth * 2.0f - 20.0f, height);
        Rect matrixRect = new Rect(rect.xMax - matrixWidth * 2.0f - 10.0f, yPos, matrixWidth, defaultElementHeight);
        Rect spriteRect = new Rect(rect.xMax - matrixWidth - 5.0f, yPos, matrixWidth, defaultElementHeight);

        EditorGUI.BeginChangeCheck();
        // draw rule tile editor GUI
        RuleInspectorOnGUI(inspectorRect, rule);
        RuleMatrixOnGUI(matrixRect, rule);
        SpriteOnGUI(spriteRect, rule);
        if (EditorGUI.EndChangeCheck())
        {
            SaveTile();
        }
    }

    /// <summary>
    /// A method to get the height of the body.
    /// </summary>
    /// <returns>Height of the body.</returns>
    private float GetElementHeight(int index)
    {
        // body height if there are tiling rules
        if (tile.tilingRules != null && tile.tilingRules.Count > 0)
        {
            if (tile.tilingRules[index].output == RuleTile.TilingRule.OutputSprite.Random)
            {
                return defaultElementHeight + singleLineHeight * (tile.tilingRules[index].sprites.Length + 2) + paddingBetweenRules;
            }
        }

        return defaultElementHeight + paddingBetweenRules;
    }

    /// <summary>
    /// A method called every time the tiling rules list is updated.
    /// </summary>
    private void ListUpdated(ReorderableList list)
    {
        SaveTile();
    }

    /// <summary>
    /// A callback method for adding new objects to the list.
    /// </summary>
    private void OnAdd(ReorderableList list)
    {
        list.list.Add(new RuleTile.TilingRule(tile.defaultColliderType));
    }

    public override void OnInspectorGUI()
    {
        tile.defaultSprite = EditorGUILayout.ObjectField("Default Sprite", tile.defaultSprite, typeof(Sprite), false) as Sprite;
        tile.defaultColliderType = (Tile.ColliderType)EditorGUILayout.EnumPopup("Default Collider", tile.defaultColliderType);
        EditorGUILayout.Space();

        if (reorderableList != null && tile.tilingRules != null)
        {
            reorderableList.DoLayoutList();
        }

        SaveTile();
    }

    /// <summary>
    /// A method to draw the rule inspector GUI.
    /// </summary>
    /// <param name="tilingRule">The rule being drawn.</param>
    private static void RuleInspectorOnGUI(Rect rect, RuleTile.TilingRule tilingRule)
    {
        float y = rect.yMin;
        EditorGUI.BeginChangeCheck();
        // draw collider field
        // TO-DO: fix minor bug default collider is not default
        GUI.Label(new Rect(rect.xMin, y, labelWidth, singleLineHeight), "Collider");
        tilingRule.colliderType = (Tile.ColliderType)EditorGUI.EnumPopup(new Rect(rect.xMin + labelWidth, y, rect.width - labelWidth, singleLineHeight), tilingRule.colliderType);
        y += singleLineHeight;
        // draw output field
        GUI.Label(new Rect(rect.xMin, y, labelWidth, singleLineHeight), "Output");
        tilingRule.output = (RuleTile.TilingRule.OutputSprite)EditorGUI.EnumPopup(new Rect(rect.xMin + labelWidth, y, rect.width - labelWidth, singleLineHeight), tilingRule.output);
        y += singleLineHeight;

        if (tilingRule.output == RuleTile.TilingRule.OutputSprite.Random)
        {
            // draw noise slider
            GUI.Label(new Rect(rect.xMin, y, labelWidth, singleLineHeight), "Noise");
            tilingRule.perlinScale = EditorGUI.Slider(new Rect(rect.xMin + labelWidth, y, rect.width - labelWidth, singleLineHeight), tilingRule.perlinScale, 0.001f, 0.999f);
            y += singleLineHeight;
        }

        if (tilingRule.output != RuleTile.TilingRule.OutputSprite.Single)
        {
            GUI.Label(new Rect(rect.xMin, y, labelWidth, singleLineHeight), "Size");
            EditorGUI.BeginChangeCheck();
            int newLength = EditorGUI.IntField(new Rect(rect.xMin + labelWidth, y, rect.width - labelWidth, singleLineHeight), tilingRule.sprites.Length);

            if (EditorGUI.EndChangeCheck())
            {
                Array.Resize(ref tilingRule.sprites, Math.Max(newLength, 1));
            }

            y += singleLineHeight;

            for (int i = 0; i < tilingRule.sprites.Length; i++)
            {
                tilingRule.sprites[i] = EditorGUI.ObjectField(new Rect(rect.xMin + labelWidth, y, rect.width - labelWidth, singleLineHeight), tilingRule.sprites[i], typeof(Sprite), false) as Sprite;
                y += singleLineHeight;
            }
        }

    }

    /// <summary>
    /// A method to draw the rule matrix GUI.
    /// </summary>
    /// <param name="tilingRule">The rule being drawn.</param>
    private static void RuleMatrixOnGUI(Rect rect, RuleTile.TilingRule tilingRule)
    {
        Handles.color = new Color(0.0f, 0.0f, 0.0f, 0.2f);
        int index = 0;
        float w = rect.width / 3.0f;
        float h = rect.height / 3.0f;

        // draws matrix
        for (int y = 0; y <= 3; y++)
        {
            float top = rect.yMin + y * h;
            Handles.DrawLine(new Vector3(rect.xMin, top), new Vector3(rect.xMax, top));
        }
        for (int x = 0; x <= 3; x++)
        {
            float left = rect.xMin + x * w;
            Handles.DrawLine(new Vector3(left, rect.yMin), new Vector3(left, rect.yMax));
        }

        Handles.color = Color.white;

        // draws inside matrix
        for (int y = 0; y <= 2; y++)
        {
            for (int x = 0; x <= 2; x++)
            {
                Rect r = new Rect(rect.xMin + x * w, rect.yMin + y * h, w - 1, h - 1);
                if (x != 1 || y != 1)
                {
                    switch (tilingRule.neighbours[index])
                    {
                        case RuleTile.TilingRule.Neighbour.This:
                            GUI.DrawTexture(r, greenCheckmark);
                            break;
                        case RuleTile.TilingRule.Neighbour.NotThis:
                            GUI.DrawTexture(r, redX);
                            break;
                        case RuleTile.TilingRule.Neighbour.AnotherTile:
                            GUI.DrawTexture(r, blueBox);
                            break;
                    }

                    if (Event.current.type == EventType.MouseDown && r.Contains(Event.current.mousePosition))
                    {
                        tilingRule.neighbours[index] = (RuleTile.TilingRule.Neighbour)(((int)tilingRule.neighbours[index] + 1) % 4);
                        GUI.changed = true;
                        Event.current.Use();
                    }

                    index++;
                }
                else
                {
                    GUI.DrawTexture(r, blackCircle);
                }
            }
        }
    }

    /// <summary>
    /// A method to draw the sprite field GUI.
    /// </summary>
    /// <param name="tilingRule">The rule being drawn.</param>
    private void SpriteOnGUI(Rect rect, RuleTile.TilingRule tilingRule)
    {
        tilingRule.sprites[0] = EditorGUI.ObjectField(new Rect(rect.xMax - rect.height, rect.yMin, rect.height, rect.height), tilingRule.sprites[0], typeof(Sprite), false) as Sprite;
    }

    /// <summary>
    /// A method to save tiling rule changes to disk and repaints scene.
    /// </summary>
    private void SaveTile()
    {
        EditorUtility.SetDirty(target);     // marks rule tile as dirty (needs to be saved to disk)
        SceneView.RepaintAll();
    }
}
