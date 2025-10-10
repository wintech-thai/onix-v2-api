#!/usr/bin/env ruby

require 'net/http'
require 'uri'
require 'json'
require './utils'

$stdout.sync = true

################### Main #######################
load_env(".env")

orgId = ENV['API_ORG']

### AddItem
apiAddItemUrl = "api/Item/org/#{orgId}/action/AddItem"
item =  {
  Code: "EMG-002",
  Description: "This is product description # 2",
  Tags: "ทดสอบ,dddd,bbb,food,feed",
  Content: "This is very long content",
  PropertiesObj: { DimensionUnit: 'gram' } ,
  Narratives: ['วันนี้เป็นวันดี', 'สินค้านี้เพื่ออะไรนะ', 'ทดสอบให้หนักๆนะจ๊ะ', 'helloworld']
}

result = make_request(:post, apiAddItemUrl, item)
puts("Added item status : [#{result['status']}] [#{result['description']}]")
