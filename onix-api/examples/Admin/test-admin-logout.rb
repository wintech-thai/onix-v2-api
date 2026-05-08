#!/usr/bin/env ruby

require 'net/http'
require 'uri'
require 'json'
require './utils'

$stdout.sync = true

################### Main #######################
load_env("../.env")

orgId = "global"
keyFile = ".token"

apiUrl = "api/OnlyUser/org/#{orgId}/action/Logout"
param =  {
  UserName: "#{ENV['USER_NAME']}",
  Password: "#{ENV['USER_PASSWORD']}",
}

ENV['API_KEY'] = nil # ถ้าไม่ใช้ API KEY ก็เซ็ตเป็น nil

result = make_request(:post, apiUrl, param)
#puts(result)

token = result["token"]["access_token"]
refreshToken = result["token"]["refresh_token"]

File.write(keyFile, token)

puts(token)
puts("======")
# ตรงนี้เป็นตัวอย่างให้ดูเรื่องการ refresh token โดยที่จริงแล้ว token มันจะหมดอายุใน 15 นาที เราก็ต้อง refresh token เพื่อเอา token ใหม่มาใช้ต่อไปเรื่อยๆ
# วิธีการก็เหมือนกับที่เคยทำมาใน please protect นั่นแหละ แต่เปลี่ยนเป็น admin-api แทน
apiUrl = "admin-api/AuthAdmin/org/#{orgId}/action/Refresh"
param =  {
  RefreshToken: refreshToken,
}
result = make_request(:post, apiUrl, param)

token = result["token"]["access_token"]
puts(token)
File.write(keyFile, token)