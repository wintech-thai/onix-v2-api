#!/usr/bin/env ruby

require 'net/http'
require 'uri'
require 'json'
require './utils'

$stdout.sync = true

################### Main #######################
load_env("../.env")

orgId = ENV['API_ORG']

apiUrl = "api/InventoryLocation/org/#{orgId}/action/AddInventoryLocation"
param = {
  Code: "LOCATION01",
  Name: "คลังพัสดุอุปกรณ์",
  LocationType: "Warehouse",
}

result = make_request(:post, apiUrl, param)

json = result.to_json
puts(json)
