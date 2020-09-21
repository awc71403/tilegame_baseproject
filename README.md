# Tile-Based Base Project - Fall 2020

DONT PANIC AND MAKE GAMES !!!

# About the Base Project

The base project includes an easy way to create a map, create character stats, place characters, and create an attack system. The base project already has a tile highlighting system for movement, movement ranges, and attack ranges.

# Implementation Summary:
The way the base project functions is all the tiles are placed using the top left corner as the starting tile. The tile placements are read from a .txt file and can be edited easily for different maps. Each tile has a highlighter animation equipped and has a variable to hold any characters on the tile. There are different selection states which are basically a bunch of if/else statements. The movement system is a simple recursive algorithm to find paths and highlights them all. If the tile is highlighted while you have selected a character, then you can move to it. Most of the movement/attack options are based around if the tile is highlighted or not.
