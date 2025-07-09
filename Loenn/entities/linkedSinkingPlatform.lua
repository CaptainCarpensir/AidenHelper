local resortPlatformHelper = require("helpers.resort_platforms")
local utils = require("utils")

local textures = {
    "default", "cliffside"
}
local textureOptions = {}

for _, texture in ipairs(textures) do
    textureOptions[utils.titleCase(texture)] = texture
end

local linkedSinkingPlatform = {}

linkedSinkingPlatform.name = "AidenHelper/LinkedSinkingPlatform"
linkedSinkingPlatform.depth = 1
linkedSinkingPlatform.nodeLimits = {1,1}
linkedSinkingPlatform.nodeLineRenderType = "line"
linkedSinkingPlatform.fieldInformation = {
    texture = {
        options = textureOptions
    }
}

linkedSinkingPlatform.placements = {
    name = "linkedSinkingPlatform",
    data = {
        flag = "",
        outerColor = "2a1923",
        innerColor = "160b12",
        surfaceIndex = 15,
        width = 8,
        reversed = false,
        returnSynced = false,
        texture = "default"
    }
}

function linkedSinkingPlatform.sprite(room, entity)
    local sprites = {}

    local x, y = entity.x or 0, entity.y or 0
    local nodes = entity.nodes or {{x = x, y = y+16}}
    local nodeY = nodes[1].y

    resortPlatformHelper.addConnectorSprites(sprites, entity, x, y, x, nodeY)
    resortPlatformHelper.addPlatformSprites(sprites, entity, entity)

    return sprites
end

function linkedSinkingPlatform.nodeSprite(room, entity, node)
    local normalizedNode = node
    normalizedNode.x = entity.x

    return resortPlatformHelper.addPlatformSprites({}, entity, normalizedNode)
end

function linkedSinkingPlatform.selection(room, entity)
    local x, y = entity.x or 0, entity.y or 0
    local width = entity.width or 16
    local nodes = entity.nodes or {}

    local mainRectangle = utils.rectangle(x, y, width, 8)
    local nodeRectangles = {}

    for i, node in ipairs(nodes) do
        nodeRectangles[i] = utils.rectangle(x, node.y, width, 8)
    end

    return mainRectangle, nodeRectangles
end

function linkedSinkingPlatform.onMove(room, entity, nodeIndex, offsetX, offsetY)
    if nodeIndex == 0 and entity.y + 8 + offsetY > entity.nodes[1].y then
        return false
    end
    if nodeIndex == 1 and (entity.y + 8 > entity.nodes[1].y + offsetY or offsetX ~= 0) then
        return false
    end

    return true
end

return linkedSinkingPlatform