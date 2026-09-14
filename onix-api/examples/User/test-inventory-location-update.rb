#!/usr/bin/env ruby

require 'net/http'
require 'uri'
require 'json'
require './utils'

$stdout.sync = true

################### Main #######################
load_env("../.env")

orgId = ENV['API_ORG']
id = 'c7324536-67b6-420a-97bc-deea9178f1b3'

param = {
  Name: "คลังพัสดุอุปกรณ์ (แก้ไข)",
  LocationType: "Warehouse",
}

apiUrl = "api/InventoryLocation/org/#{orgId}/action/UpdateInventoryLocationById/#{id}"
result = make_request(:post, apiUrl, param)

puts(result)
