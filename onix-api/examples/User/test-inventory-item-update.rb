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
  NameTh: "กาวลาเท็กซ์ TOA 1 กก. (แก้ไข)",
  ItemType: "RawMaterial",
  Unit: "Bottle",
  Price: 130.0,
  IsVatIncluded: true,
}

apiUrl = "api/InventoryItem/org/#{orgId}/action/UpdateInventoryItemById/#{id}"
result = make_request(:post, apiUrl, param)

puts(result)
