﻿module AidenHelperLinkedPlatforms

using ..Ahorn, Maple

@mapdef Entity "AidenHelper/LinkedSinkingPlatform" LinkedSinkingPlatform(x::Integer, y::Integer, width::Integer=16, surfaceIndex::Integer=15, texture::String="default", flag::String="", reversed::Bool=false, outerColor::String="2a1923", innerColor::String="160b12")

const placements = Ahorn.PlacementDict(
	"Linked Sinking Platform (AidenHelper)" => Ahorn.EntityPlacement(
		LinkedSinkingPlatform,
		"rectangle",
        Dict{String,Any}(),
        function (entity)
            entity.data["nodes"] = [(
                Int(entity.data["x"]),
                Int(entity.data["y"]) + 32,
            )]
        end,
    ),
)

Ahorn.editingOptions(entity::LinkedSinkingPlatform) = Dict{String, Any}(
    "texture" => Maple.wood_platform_textures
)
Ahorn.resizable(entity::LinkedSinkingPlatform) = true, false
Ahorn.nodeLimits(entity::LinkedSinkingPlatform) = 1, 1
Ahorn.minimumSize(entity::LinkedSinkingPlatform) = 8, 0

function Ahorn.selection(entity::LinkedSinkingPlatform)
    width = Int(get(entity.data, "width", 8))
    startX, startY = Int(entity.data["x"]), Int(entity.data["y"])
    stopX, stopY = Int.(entity.data["nodes"][1])

    return [Ahorn.Rectangle(startX, startY, width, 8), Ahorn.Rectangle(startX, stopY, width, 8)]
end

outerColor = (30, 14, 25) ./ 255
innerColor = (10, 0, 6) ./ 255

function renderConnection(ctx::Ahorn.Cairo.CairoContext, x::Int, y::Int, nx::Int, ny::Int, width::Int)
    cx, cy = x + floor(Int, width / 2), y + 4
    cnx, cny = nx + floor(Int, width / 2), ny + 4

    length = sqrt((x - nx)^2 + (y - ny)^2)
    theta = atan(cny - cy, cnx - cx)

    Ahorn.Cairo.save(ctx)

    Ahorn.translate(ctx, cx, cy)
    Ahorn.rotate(ctx, theta)

    Ahorn.setSourceColor(ctx, outerColor)
    Ahorn.set_antialias(ctx, 1)
    Ahorn.set_line_width(ctx, 3);

    Ahorn.move_to(ctx, 0, 0)
    Ahorn.line_to(ctx, length, 0)

    Ahorn.stroke(ctx)

    Ahorn.setSourceColor(ctx, innerColor)
    Ahorn.set_antialias(ctx, 1)
    Ahorn.set_line_width(ctx, 1);

    Ahorn.move_to(ctx, 0, 0)
    Ahorn.line_to(ctx, length, 0)

    Ahorn.stroke(ctx)

    Ahorn.Cairo.restore(ctx)
end

function renderPlatform(ctx::Ahorn.Cairo.CairoContext, texture::String, x::Int, y::Int, width::Int)
    tilesWidth = div(width, 8)

    for i in 2:tilesWidth - 1
        Ahorn.drawImage(ctx, "objects/woodPlatform/$texture", x + 8 * (i - 1), y, 8, 0, 8, 8)
    end

    Ahorn.drawImage(ctx, "objects/woodPlatform/$texture", x, y, 0, 0, 8, 8)
    Ahorn.drawImage(ctx, "objects/woodPlatform/$texture", x + tilesWidth * 8 - 8, y, 24, 0, 8, 8)
    Ahorn.drawImage(ctx, "objects/woodPlatform/$texture", x + floor(Int, width / 2) - 4, y, 16, 0, 8, 8)
end

function Ahorn.renderAbs(ctx::Ahorn.Cairo.CairoContext, entity::LinkedSinkingPlatform, room::Maple.Room)
    width = Int(get(entity.data, "width", 8))

    x, y = Int(entity.data["x"]), Int(entity.data["y"])
    
    texture = get(entity.data, "texture", "default")

    renderConnection(ctx, x, y, x, Int(Ahorn.loadedState.room.size[2]), width)
    renderPlatform(ctx, texture, x, y, width)
end

function Ahorn.renderSelectedAbs(ctx::Ahorn.Cairo.CairoContext, entity::LinkedSinkingPlatform, room::Maple.Room)
    width = Int(get(entity.data, "width", 8))

    startX, startY = Int(entity.data["x"]), Int(entity.data["y"])
    stopX, stopY = Int.(entity.data["nodes"][1])

    texture = get(entity.data, "texture", "default")

    renderPlatform(ctx, texture, startX, startY, width)
    renderPlatform(ctx, texture, startX, stopY, width)

    Ahorn.drawArrow(ctx, startX + width / 2, startY, startX + width / 2, stopY, Ahorn.colors.selection_selected_fc, headLength=6)
end

end