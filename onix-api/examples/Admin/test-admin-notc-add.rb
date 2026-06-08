#!/usr/bin/env ruby

require 'net/http'
require 'uri'
require 'json'
require './utils'

$stdout.sync = true

################### Main #######################
load_env("../.env")

orgId = ENV['API_ORG']
keyFile = ".token"

hhmmss = Time.now.strftime("%H%M%S")

### 
apiUrl = "admin-api/AdminNotiChannel/org/global/action/AddNotiChannel"
param =  {
  ChannelName: "Payment notification #{hhmmss}",
  Description: "Test notification channel created at #{hhmmss}",
  Type: "Telegram",
  Status: "Enabled",
  TelegramWebhookUrl: "https://api.telegram.org/bot<token>",
  TelegramChatId: "9090009999999xxxxxxx",
  EventTypes: [ 'Payment.Success', 'Payment.Failed' ],
}

token = File.read(keyFile)

ENV['API_KEY'] = nil # ถ้าไม่ใช้ API KEY ก็เซ็ตเป็น nil
ENV['ACCESS_TOKEN'] = token

#puts("===[#{token}]")

result = make_request(:post, apiUrl, param)
puts(result.to_json)
