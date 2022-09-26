local drawableSprite = require("structs.drawable_sprite")
local utils = require("utils")

local textures = {
    default = "objects/temple/dashButton00",
    mirror = "objects/temple/dashButtonMirror00",
}
local textureOptions = {}

for texture, _ in pairs(textures) do
    textureOptions[utils.titleCase(texture)] = texture
end

local dirOptions = {
    "Up",
    "Down",
    "Left",
    "Right"
}

local linkedDashSwitch = {}

linkedDashSwitch.name = "AidenHelper/LinkedDashSwitch"
linkedDashSwitch.texture = "objects/temple/dashButton00"
linkedDashSwitch.depth = 0
linkedDashSwitch.justification = {0.5, 0.5}
linkedDashSwitch.fieldInformation = {
    sprite = {
        options = textureOptions
    },
    direction = {
        options = dirOptions,
        editable = false
    }
}
linkedDashSwitch.placements = {}

function linkedDashSwitch.sprite(room, entity)
    local direction = entity.direction
    local texture = entity.sprite == "default" and textures["default"] or textures["mirror"]
    local sprite = drawableSprite.fromTexture(texture, entity)

    if direction == "Up" then
        sprite:addPosition(8, 8)
        sprite.rotation = math.pi/2
    end

    if direction == "Down" then
        sprite:addPosition(8, 0)
        sprite.rotation = -math.pi/2
    end

    if direction == "Left" then
        sprite:addPosition(8, 8)
        sprite.rotation = 0
    end

    if direction == "Right" then
        sprite:addPosition(0, 8)
        sprite.rotation = math.pi
    end

    return sprite
end

for _, direction in ipairs(dirOptions) do
    local placement = {
        name = string.format("Linked Dash Switch (%s)", direction),
        data = {
            flag = "",
            persistent = false,
            sprite = "default",
            reversed = false,
            direction = direction
        }
    }

    table.insert(linkedDashSwitch.placements, placement)
end

return linkedDashSwitch